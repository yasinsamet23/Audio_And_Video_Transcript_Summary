import openai
import sys

openai.api_key = "Enter API Key"

audio_path = sys.argv[1]

with open(audio_path, "rb") as audio_file:
    transcript = openai.audio.transcriptions.create(
        file=audio_file,
        model="whisper-1"
    )

print(transcript.text)
