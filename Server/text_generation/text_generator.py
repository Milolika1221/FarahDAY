import subprocess
import os
import random

class TextGenerator:
    def __init__(self):
        self.dir_path = 'text_generation'
        
    def get_response(self, text, 
                    llama_path=None, 
                    model_path=None, 
                    prompt_path=None):
        if llama_path is None:
            llama_path = os.path.join(self.dir_path, "llamacpp/llama-cli.exe")
        if model_path is None:
            model_path = os.path.join(self.dir_path, "models/model.gguf")
        if prompt_path is None:
            prompt_path = os.path.join(self.dir_path, "prompts/default.txt")
        with open(prompt_path, 'r', encoding='utf-8') as file:
            file_content = file.read()
        prompt = file_content.replace('{prompt}', text)
        prompt_file_name = os.path.join(self.dir_path, f"prompts/in prompts/{str(random.random()).replace('.', '')}.txt")
        with open(prompt_file_name, 'w', encoding='utf-8') as file:
            file.write(prompt)
        command = [
            llama_path,
            "-m", model_path,
            "--file", prompt_file_name,
            "-n", "200"
        ]
        res_text = "### Response: Здравствуйте. Благодарим за обращение. Ваш запрос переден в службу поддержки. [end of text]"
        try:
            res_text = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True, encoding='utf-8').stdout
        except Exception as e:
            print(f"Ошибка генерации: {e}")
        os.remove(prompt_file_name)
        start = res_text.find('### Response:') + len('### Response:')
        end = res_text.find('[end of text]')
        result = res_text[start:end].strip()
        return result

if __name__ == "__main__":
    textgen = TextGenerator()
    response = textgen.get_response("Здравствуйте! Я пытаюсь зарегистрировать аккаунт, но получаю сообщение об ошибке. Можете помочь разобраться? Спасибо!")
    print(response)