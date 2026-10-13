@echo off
chcp 65001 >nul
title Server Guida Duomo
cd /d "%~dp0"

set PYTHON=C:\Users\Asus\miniconda3\envs\ollama\python.exe
set MODELLO=gemma3:4b
set LOG=server.log

echo [%date% %time%] Avvio > "%LOG%"

echo [1/3] Controllo Ollama... >> "%LOG%"
set TENTATIVI=0
set AVVIATO=0

:check_ollama
curl -s http://localhost:11434/api/tags >nul 2>&1
if %errorlevel%==0 goto ollama_ok

if %AVVIATO%==0 (
    echo      Ollama non attivo, lo avvio... >> "%LOG%"
    start "" /b ollama serve >nul 2>&1
    set AVVIATO=1
)

set /a TENTATIVI+=1
if %TENTATIVI% GEQ 15 (
    echo ERRORE: Ollama non risponde. >> "%LOG%"
    if not "%1"=="silent" pause
    exit /b 1
)
ping -n 3 127.0.0.1 >nul
goto check_ollama

:ollama_ok
echo      Ollama attivo. >> "%LOG%"

echo [2/3] Precarico il modello %MODELLO% in GPU... >> "%LOG%"
curl -s http://localhost:11434/api/generate -d "{\"model\":\"%MODELLO%\",\"keep_alive\":\"30m\"}" >nul 2>&1

echo [3/3] Avvio server STT/TTS... >> "%LOG%"
set HF_HUB_OFFLINE=1
set TRANSFORMERS_OFFLINE=1
"%PYTHON%" -u stt_server.py >> "%LOG%" 2>&1

echo Il server si e' fermato. >> "%LOG%"
if not "%1"=="silent" pause