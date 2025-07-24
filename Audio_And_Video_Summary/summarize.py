import openai
import sys

# OpenAI API anahtarını ayarla
openai.api_key = "Enter API Key"

# Transkript metnini oku (komut satırından gelen 1. argüman)
with open(sys.argv[1], "r", encoding="utf-8") as file:
    input_text = file.read()

# Prompt oluştur
prompt = f"Aşağıdaki metni Türkçe olarak kısaca özetle:\n\n{input_text}\n\nÖzet:"

# GPT modeli ile özet al
response = openai.chat.completions.create(
    model="gpt-4o",
    messages=[
        {"role": "user", "content": prompt}
    ],
    temperature=0.7,
    max_tokens=1000
)

# Cevabı yazdır
print(response.choices[0].message.content)
