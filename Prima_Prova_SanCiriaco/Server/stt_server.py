import torch
torch.backends.cudnn.enabled = False  # evita il conflitto con le DLL cuDNN di sistema

from fastapi import FastAPI, UploadFile, File, HTTPException
from fastapi.responses import FileResponse
from starlette.background import BackgroundTask
from pydantic import BaseModel
from faster_whisper import WhisperModel
from kokoro import KPipeline
import numpy as np
import tempfile
import wave
import re
import os

app = FastAPI()

# ----------------------------------------------------------------------
# RILEVAMENTO HARDWARE (CUDA / CPU FALLBACK)
# ----------------------------------------------------------------------
device = "cuda" if torch.cuda.is_available() else "cpu"
print(f"[Device] Hardware rilevato per inferenza: {device.upper()}", flush=True)

# ----------------------------------------------------------------------
# 1. SETUP SPEECH-TO-TEXT (faster-whisper)
# ----------------------------------------------------------------------
# Su GPU usa int8_float16 per performance/VRAM ottimali, su CPU usa int8 standard
whisper_compute = "int8_float16" if device == "cuda" else "int8"
stt_model = WhisperModel("small", device=device, compute_type=whisper_compute)

# ----------------------------------------------------------------------
# 2. SETUP TEXT-TO-SPEECH (Kokoro - Voce Italiana)
# ----------------------------------------------------------------------
KOKORO_VOICE = "if_sara"        # femminile. Alternativa maschile: "im_nicola"
KOKORO_SPEED = 0.95             # <1 = più posata, adatta a una guida
KOKORO_SAMPLE_RATE = 24000

# Fallback automatico: se cuda non è disponibile, inizializza su cpu
kokoro = KPipeline(lang_code="i", repo_id="hexgrad/Kokoro-82M", device=device)

# Riscaldamento: la prima sintesi è lenta, la facciamo all'avvio
print("[TTS] Riscaldamento Kokoro...", flush=True)
for _ in kokoro("Test di avvio.", voice=KOKORO_VOICE):
    pass
print("[TTS] Kokoro pronto.", flush=True)


def pulisci_testo(testo: str) -> str:
    """Toglie markdown e simboli che il TTS leggerebbe male."""
    testo = re.sub(r"[*#_`~>]", "", testo)
    testo = re.sub(r"\s+", " ", testo)
    return testo.strip()


# ----------------------------------------------------------------------
# ENDPOINTS
# ----------------------------------------------------------------------
class TTSRequest(BaseModel):
    text: str


@app.get("/health")
async def health():
    return {"status": "ok"}


@app.post("/transcribe")
async def transcribe_audio(file: UploadFile = File(...)):
    with tempfile.NamedTemporaryFile(delete=False, suffix=".wav") as tmp:
        contents = await file.read()
        tmp.write(contents)
        tmp_path = tmp.name

    try:
        segments, _ = stt_model.transcribe(
            tmp_path,
            language="it",
            beam_size=5,
            vad_filter=True,
            initial_prompt="Duomo di San Ciriaco, Ancona, Colle Guasco, cattedrale, cripta, protiro, leoni, Afrodite Euplea, cupola."
        )
        text = " ".join([s.text for s in segments]).strip()
        print(f"[STT Trascritto]: {text}", flush=True)
        return {"text": text}
    finally:
        if os.path.exists(tmp_path):
            os.remove(tmp_path)


@app.post("/tts")
async def text_to_speech(req: TTSRequest):
    testo = pulisci_testo(req.text)
    print(f"[TTS Sintesi]: {testo}", flush=True)

    if not testo:
        raise HTTPException(status_code=400, detail="Testo vuoto")

    pezzi = []
    for _, _, audio in kokoro(testo, voice=KOKORO_VOICE, speed=KOKORO_SPEED):
        if torch.is_tensor(audio):
            audio = audio.detach().cpu().numpy()
        pezzi.append(audio)

    if not pezzi:
        raise HTTPException(status_code=500, detail="Nessun audio generato")

    pcm = np.concatenate(pezzi)
    pcm = (np.clip(pcm, -1.0, 1.0) * 32767).astype(np.int16)

    tmp = tempfile.NamedTemporaryFile(delete=False, suffix=".wav")
    tmp.close()
    output_file = tmp.name

    with wave.open(output_file, "wb") as wav_file:
        wav_file.setnchannels(1)
        wav_file.setsampwidth(2)
        wav_file.setframerate(KOKORO_SAMPLE_RATE)
        wav_file.writeframes(pcm.tobytes())

    print(f"[TTS Generato]: {output_file} ({os.path.getsize(output_file)} bytes)", flush=True)

    return FileResponse(
        output_file,
        media_type="audio/wav",
        background=BackgroundTask(os.remove, output_file),
    )


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="127.0.0.1", port=5000)