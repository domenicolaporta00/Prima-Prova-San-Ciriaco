@echo off
chcp 65001 >nul
title Server Guida Duomo
cd /d "%~dp0"

set "PYTHON=%~dp0python\python.exe"
set "OLLAMA=%~dp0ollama\ollama.exe"
set "OLLAMA_MODELS=%~dp0ollama_models"
set "OLLAMA_HOST=127.0.0.1:11435"
set "HF_HOME=%~dp0hf_cache"
::set "HF_HUB_OFFLINE=1"
::set "TRANSFORMERS_OFFLINE=1"
set "PATH=%~dp0python;%~dp0python\Library\bin;%PATH%"
set "MODELLO=gemma3:4b"
set "LOG=%~dp0server.log"

echo [%date% %time%] Avvio > "%LOG%"

if not exist "%PYTHON%" (
    echo ERRORE: Python non trovato: %PYTHON% >> "%LOG%"
    if not "%1"=="silent" pause
    exit /b 1
)
if not exist "%OLLAMA%" (
    echo ERRORE: Ollama non trovato: %OLLAMA% >> "%LOG%"
    if not "%1"=="silent" pause
    exit /b 1
)

echo [1/3] Controllo Ollama... >> "%LOG%"
set TENTATIVI=0
set AVVIATO=0

:check_ollama
curl -s http://127.0.0.1:11435/api/tags >nul 2>&1
if %errorlevel%==0 goto ollama_ok

if %AVVIATO%==0 (
    echo       Ollama non attivo, lo avvio... >> "%LOG%"
    start "" /b "%OLLAMA%" serve >nul 2>&1
    set AVVIATO=1
)

set /a TENTATIVI+=1
if %TENTATIVI% GEQ 20 (
    echo ERRORE: Ollama non risponde. >> "%LOG%"
    if not "%1"=="silent" pause
    exit /b 1
)
ping -n 1 127.0.0.1 >nul
goto check_ollama

:ollama_ok
echo       Ollama attivo. >> "%LOG%"

echo [2/3] Precaricamento asincrono di %MODELLO%... >> "%LOG%"
:: Eseguito in background con start /b: non blocca l'avvio di Python
start "" /b curl -s --max-time 120 http://127.0.0.1:11435/api/generate -d "{\"model\":\"%MODELLO%\",\"keep_alive\":\"30m\"}" >nul 2>&1

echo [3/3] Avvio immediato server STT/TTS... >> "%LOG%"
"%PYTHON%" -u stt_server.py >> "%LOG%" 2>&1

echo Il server si e' fermato. >> "%LOG%"
if not "%1"=="silent" pause