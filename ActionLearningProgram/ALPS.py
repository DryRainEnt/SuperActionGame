import time

print('Learning environment setup.')
start_time = time.time()

import os
import json
import socket
import numpy as np
import torch
import torch.nn as nn
import torch.optim as optim

print('import done.')


# 신경망 A: 14개의 입력을 받아 5개의 의도 요인을 출력
class NetA(nn.Module):
    def __init__(self):
        super(NetA, self).__init__()
        self.fc1 = nn.Linear(12, 60)
        self.fc2 = nn.Linear(60, 24)
        self.fc3 = nn.Linear(24, 5)

    def forward(self, x):
        x = torch.relu(self.fc1(x))
        x = torch.relu(self.fc2(x))
        x = torch.tanh(self.fc3(x))  # -1 ~ 1 사이의 값으로 출력
        return x


# 신경망 B: 5개의 의도 요인을 받아 1개의 기대 보상치를 출력
class NetB(nn.Module):
    def __init__(self):
        super(NetB, self).__init__()
        self.fc1 = nn.Linear(5, 25)
        self.fc2 = nn.Linear(25, 1)

    def forward(self, x):
        x = torch.relu(self.fc1(x))
        x = self.fc2(x)
        return x


def init_weights_he(m):
    if type(m) == nn.Linear:
        torch.nn.init.kaiming_normal_(m.weight, mode='fan_in', nonlinearity='relu')
        m.bias.data.fill_(0)


level = 0

print('network instance preparing...')

# 인스턴스 생성
net_a = NetA()
net_b = NetB()

# He Initialization 적용
net_a.apply(init_weights_he)
net_b.apply(init_weights_he)

# weights.json 파일 존재 여부 확인 및 처리
weights_file_path = 'Weights/weights.json'
if not os.path.exists(weights_file_path):
    print("weights.json 파일이 존재하지 않습니다. 새로운 파일을 생성합니다.")

    # 디렉토리 확인 후 없으면 생성
    if not os.path.exists('Weights'):
        os.makedirs('Weights')

    # 가중치 초기화
    weights = {
        'level': 0,
        'net_a_fc1_bias': net_a.fc1.bias.detach().numpy().tolist(),
        'net_a_fc1_weight': net_a.fc1.weight.detach().numpy().tolist(),
        'net_a_fc2_bias': net_a.fc2.bias.detach().numpy().tolist(),
        'net_a_fc2_weight': net_a.fc2.weight.detach().numpy().tolist(),
        'net_a_fc3_bias': net_a.fc3.bias.detach().numpy().tolist(),
        'net_a_fc3_weight': net_a.fc3.weight.detach().numpy().tolist(),
        'net_b_fc1_bias': net_b.fc1.bias.detach().numpy().tolist(),
        'net_b_fc1_weight': net_b.fc1.weight.detach().numpy().tolist(),
        'net_b_fc2_bias': net_b.fc2.bias.detach().numpy().tolist(),
        'net_b_fc2_weight': net_b.fc2.weight.detach().numpy().tolist()
    }

    # 가중치 파일 저장
    with open(weights_file_path, 'w') as f:
        json.dump(weights, f, indent=4)
else:
    print("weights.json 파일이 존재합니다. 파일을 불러옵니다.")

    # 가중치 파일 로드
    with open('Weights/weights.json', 'r') as f:
        weights = json.load(f)
        level = weights['level']
        net_a.fc1.bias = nn.Parameter(torch.FloatTensor(weights['net_a_fc1_bias']))
        net_a.fc1.weight = nn.Parameter(torch.FloatTensor(weights['net_a_fc1_weight']))
        net_a.fc2.bias = nn.Parameter(torch.FloatTensor(weights['net_a_fc2_bias']))
        net_a.fc2.weight = nn.Parameter(torch.FloatTensor(weights['net_a_fc2_weight']))
        net_a.fc3.bias = nn.Parameter(torch.FloatTensor(weights['net_a_fc3_bias']))
        net_a.fc3.weight = nn.Parameter(torch.FloatTensor(weights['net_a_fc3_weight']))
        net_b.fc1.bias = nn.Parameter(torch.FloatTensor(weights['net_b_fc1_bias']))
        net_b.fc1.weight = nn.Parameter(torch.FloatTensor(weights['net_b_fc1_weight']))
        net_b.fc2.bias = nn.Parameter(torch.FloatTensor(weights['net_b_fc2_bias']))
        net_b.fc2.weight = nn.Parameter(torch.FloatTensor(weights['net_b_fc2_weight']))


print('ALPS now ready. Time elapsed: % 2.4f' % (time.time() - start_time))
start_time = time.time()

run = True

host = '127.0.0.1'
port = 5000
server_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)

server_socket.bind((host, port))
server_socket.listen(1)

print("Server is listening on port:", port)

conn, address = server_socket.accept()
print(f"Connection from {address} has been established!")


print('Learning Process Initiated. Send \'s\' to start learning, \'q\' to quit.')
while run:

    print('Waiting for data...')
    conn.sendall(b'Waiting>')
    while True:
        socket_data = conn.recv(1024)
        if socket_data:
            print(f"Received data: {socket_data.decode()}")

            line = socket_data.decode().strip()
            if line == 'quit' or line == 'q':
                run = False
                break
            if line == 'start' or line == 's':
                break

    if not run:
        break

    start_time = time.time()

    print('Reading data...')
    with open('data.json', 'r') as f:
        data = json.load(f)

    print(f'Learning from {level}...')

    # Frames 데이터 추출
    frames = data["Frames"]

    # ActorDataSet을 NumPy 배열로 저장
    actor_data_sets = []
    validation_data_sets = []

    for frame in frames:
        actor_data_set = frame["ActorDataSet"]
        actor_data_sets.append(actor_data_set)

        validation_data_set = frame["Validation"]
        validation_data_sets.append(validation_data_set)

    actor_data_sets = np.array(actor_data_sets)

    # 데이터를 Torch Tensor로 변환
    input_data = torch.FloatTensor(actor_data_sets)
    real_rewards = torch.FloatTensor(validation_data_sets) # 여기는 실제 보상 데이터를 사용해야 함

    # Optimizer 및 loss 함수 설정
    optimizer_a = optim.Adam(net_a.parameters(), lr=0.001)
    optimizer_b = optim.Adam(net_b.parameters(), lr=0.001)
    criterion = nn.MSELoss()

    reward_gaps = []

    # 학습 시작
    for epoch in range(100):  # 100 epochs
        intention_factors = net_a(input_data)
        expected_rewards = net_b(intention_factors)
        reward_gaps.append(torch.mean(expected_rewards - real_rewards))

        loss = criterion(expected_rewards.squeeze(), real_rewards)

        optimizer_a.zero_grad()
        optimizer_b.zero_grad()
        loss.backward()
        optimizer_a.step()
        optimizer_b.step()

        if (epoch + 1) % 10 == 0:
            print(f'Epoch {epoch + 1}, Loss: {loss.item()}')

    # 학습에 걸린 시간 출력
    print(f'Learning time: {time.time() - start_time: 2.4}')

    level += 1

    # 학습 결과를 각 레이어별로 가중치와 편향치를 묶어서 json 파일로 변환한 뒤 Weights 디렉토리에 저장
    weights = {
        'level': level,
        'net_a_fc1_weight': net_a.fc1.weight.tolist(),
        'net_a_fc2_weight': net_a.fc2.weight.tolist(),
        'net_a_fc3_weight': net_a.fc3.weight.tolist(),
        'net_a_fc1_bias': net_a.fc1.bias.tolist(),
        'net_a_fc2_bias': net_a.fc2.bias.tolist(),
        'net_a_fc3_bias': net_a.fc3.bias.tolist(),
        'net_b_fc1_weight': net_b.fc1.weight.tolist(),
        'net_b_fc2_weight': net_b.fc2.weight.tolist(),
        'net_b_fc1_bias': net_b.fc1.bias.tolist(),
        'net_b_fc2_bias': net_b.fc2.bias.tolist(),
    }

    with open('Weights/weights_%i.json' % level, 'w') as f:
        json.dump(weights, f, sort_keys=True, indent=4)

    with open('Weights/weights.json', 'w') as f:
        json.dump(weights, f, sort_keys=True, indent=4)

    print(f'Done>', level)

    # Echo back the data
    conn.sendall(b'Done>')


conn.close()
server_socket.close()
