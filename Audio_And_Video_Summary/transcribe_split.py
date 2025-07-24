import os
import sys
import subprocess
import openai
from moviepy.editor import VideoFileClip
from pydub import AudioSegment

openai.api_key = "Enter API Key"  # Gerçek anahtarını burada kullan

def convert_to_supported_format(input_path):
    base, _ = os.path.splitext(input_path)
    output_path = base + "_converted.mp4"

    if not os.path.exists(output_path):
        command = [
            "ffmpeg",
            "-i", input_path,
            "-c:v", "copy",
            "-c:a", "aac",
            output_path
        ]
        subprocess.run(command, stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL)

    return output_path

def extract_audio(video_path, output_audio_path="temp_audio.wav"):
    clip = VideoFileClip(video_path)
    clip.audio.write_audiofile(output_audio_path, verbose=False, logger=None)  # Sessiz çıktı
    return output_audio_path

def split_audio(audio_path, segment_length_ms=25 * 1000):
    audio = AudioSegment.from_file(audio_path)
    chunks = []

    for i in range(0, len(audio), segment_length_ms):
        chunk = audio[i:i+segment_length_ms]
        chunk_path = f"chunk_{i//segment_length_ms}.wav"
        chunk.export(chunk_path, format="wav")
        chunks.append(chunk_path)

    return chunks

def transcribe_audio(chunk_path):
    with open(chunk_path, "rb") as audio_file:
        transcript = openai.audio.transcriptions.create(
            file=audio_file,
            model="whisper-1"
        )
    return transcript.text

def process_audio_file(video_path):
    if video_path.lower().endswith(".mkv"):
        video_path = convert_to_supported_format(video_path)

    audio_path = extract_audio(video_path)
    chunks = split_audio(audio_path)

    final_transcript = ""
    for chunk in chunks:
        # Sessiz modda çalışıyor: bilgilendirme mesajı yok
        final_transcript += transcribe_audio(chunk) + "\n"
        os.remove(chunk)

    os.remove(audio_path)
    return final_transcript

if __name__ == "__main__":
    if len(sys.argv) < 2:
        sys.exit(1)

    video_path = sys.argv[1]
    try:
        result = process_audio_file(video_path)
        print(result)  # Sadece sonuç
    except Exception as e:
        print("An error occurred:", str(e))
