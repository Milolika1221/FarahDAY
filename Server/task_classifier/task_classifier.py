import onnxruntime
import numpy as np
from tokenizers import Tokenizer
import os

class TaskClassifier:

    def __init__(self):
        dir_path = os.path.dirname(os.path.abspath(__file__))
        model_path = os.path.join(dir_path, 'task_classifier.onnx')
        tokenizer_path = os.path.join(dir_path, 'tokenizer.json')
        self.session = onnxruntime.InferenceSession(model_path)
        self.input_name = self.session.get_inputs()[0].name
        self.output_name = self.session.get_outputs()[0].name
        self.tokenizer = Tokenizer.from_file(tokenizer_path)
        self.dict = {
            0: 'варианты доставки',
            1: 'варианты оплаты',
            2: 'возврат средств',
            3: 'восстановление пароля',
            4: 'время доставки',
            5: 'выбор адреса доставки',
            6: 'изменение адреса доставки',
            7: 'изменение заказа',
            8: 'отзыв',
            9: 'отмена заказа',
            10: 'отслеживание возврата средств',
            11: 'отслеживание заказа',
            12: 'подписка на новостную рассылку',
            13: 'политика возврата',
            14: 'получение информации',
            15: 'проблемы с оплатой',
            16: 'проблемы с регистрацией',
            17: 'проверка счета',
            18: 'размещение заказа',
            19: 'редактирование учетной записи',
            20: 'связь со службой поддержки',
            21: 'смена учетной записи',
            22: 'создание учетной записи',
            23: 'удаление аккаунта'
        }

    def get_predict(self, text):
        try:
            inputs = self.tokenizer.encode(text)
            input_ids = np.array(inputs.ids, dtype=np.int64).reshape(1, -1)  
            attention_mask = np.array(inputs.attention_mask, dtype=np.int64).reshape(1, -1)
            onnx_inputs = {
                "input_ids": input_ids,
                "attention_mask": attention_mask,
            }
            onnx_outputs = self.session.run(None, onnx_inputs)
            logits = onnx_outputs[0]
            predictions = np.argmax(logits, axis=-1)
            return self.dict[predictions[0]]
        except Exception:
            return ''

if __name__ == "__main__":
    taskclassifier = TaskClassifier()
    print(taskclassifier.get_predict("Как удалить аккаунт"))
