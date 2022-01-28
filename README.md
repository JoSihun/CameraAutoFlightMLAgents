# Auto Flight ML-Agents based on Camera
This Drone Autonomous Flight based on Camera using Unity ML Agent.
본 프로젝트는 Camera를 활용한 Unity ML Agent 기반 머신러닝을 이용한 드론 자율비행 시스템으로, 장애물(사물)을 회피하여 목적지까지 자율적으로 비행하는 시스템이다. Camera를 이용하여 장애물(사물)을 탐지하고, 탐지한 장애물(사물)의 거리정보를 기반으로 Unity ML Agent Package를 이용하여 PPO(Proximal Policy Optimization) 기반의 머신러닝을 수행하여, 드론 비행 시 발생하는 장애물에 대한 회피 알고리즘을 제안하고, 테스트환경에서 수행한 뒤 성능을 확인한다.
- [Unity ML-Agent](https://github.com/Unity-Technologies/ml-agents): https://github.com/Unity-Technologies/ml-agents
- [YOLOv4 Tiny Barracuda](https://github.com/keijiro/YoloV4TinyBarracuda): https://github.com/keijiro/YoloV4TinyBarracuda





## 1. Environments

- Unity
- Unity ML Agent
- C# Script
- Anaconda3(Python 3.8.5)
- Window 10
- Intel Core i5-6600
- RAM DDR4-2133MHz(PC4-17000) 16GB
- NVIDIA GeForce GTX 1060 6GB


### 1. 1 Unity ML-Agents
- Download recent version of release branch at https://github.com/Unity-Technologies/ml-agents.  
- The location of 'ml-agents' is like below or free.  
```
ㄴroot
  ㄴImages  
  ㄴYoloFight  
  ㄴml-agents  
```


### 1. 2 YOLOv4 Tiny Barracuda
- Download YOLOv4 Tiny for Unity at https://github.com/keijiro/YoloV4TinyBarracuda.  
- The location of 'YoloV4TinyBarracuda' is like below.
```
ㄴroot
  ㄴImages  
  ㄴYoloFight  
  ㄴml-agents  
  ㄴYoloV4TinyBarracuda
```  
- For Real-Time object detection, the YOLOv4 Tiny model can be a good choice.  
- YOLOv4 Tiny is a compressed version of the YOLOv4 model, simplifying the structure by reducing parameters.  
- Therefore, it is suitable for use in mobile or embedded devices.  





## 2. Main Configuration


### 2. 1 Main Architecture
- This image shows how drone autonomous flight machine learning works.  
- Unity ML-Agents has 5 different functions below.  
**`Initialize`, `OnEpisodeBegin`, `CollectObservations`, `OnActionReceived`, `Heuristic`.**
  - Initialize  
    Initialization such as importing Unity object information.  
  - OnEpisodeBegin  
    Initialization of location and speed information of Agent, Target, etc. at the start of an episode.  
  - CollectObservations  
    Observation of environmental information for policy making.  
  - OnActionReceived  
    Conduct actions according to the determined policy
  - Heuristic  
    Manipulated by the developer to make sure the behavior works well.
- **`Heuristic`** is excluded because it is just checking function that if actions work or not.  

<p align="center"><img width="100%" src="https://user-images.githubusercontent.com/59362257/151522584-ee581411-754b-4d61-8ef9-16a2429de2e2.png"/></p>

- First, get Environment Informations from `Environment` such as `Map Information`, `Target Position`, `Agent Position` etc.  
- Second, YOLOv4 Tiny from Agent detect objects and get detected object data, such as `size`, `position`, `distance` etc.
- Then, update the reward and modify the behavior to get better reward from `Unity ML Agent`.  
- During Learning, the learning information is transmitted to the `MonitoringUI`.  


### 2. 2 Sub Arhitecture
- This image shows how `Agent` learns from `Unity ML Agent`.

<p align="center"><img width="75%" src="https://user-images.githubusercontent.com/59362257/151524351-5fcd0a75-627f-468b-9f3e-aa6e3bc4a7e3.png"/></p>

- A Behavior is selected automatically based on reward in Communiator from Unity ML Agent.  
- The Drone performs actions and detects obstacles by two cameras.  
- After determining whether the object detected by the camera is an obstacle or a target,  
a reward is set according to the measured distance information.


### 2. 3 Stereo Camera Triangulation
- This Autonomous Flight Simulation is based on Stereo Camera Triangulation system.  

<p align="center"><img width="50%" src="https://user-images.githubusercontent.com/59362257/151525158-6cf1773f-26d3-470a-9f1c-5d689dd6b91a.png" /></p>

- 양안식 스테레오 비전 시스템은 삼각측량을 기반으로 양안식 측정 방법을 통해 목표 물체의 3차원 정보를 얻을 수 있다.  
- P는 목표지점, OL과 OR은 동일한 초점거리 카메라의 시축 센터(Visual Center), PL과 PR은 각각 목표물을 바라보는 투시점,  
xL과 xR은 카메라 이미지의 PL과 PR에 대응하는 왼쪽과 오른쪽의 수평 좌표계이다.
- 양안식 스테레오 비전 측정 방법에 따라 OL, OR, PL, PR은 각각 이미지의 좌표를 의미하며,  
  B는 두 카메라의 이격거리, f는 카메라의 초점거리, xL - xR은 disparity(d)로 정의된 시차오차,  
  D는 카메라와 목표물(P)까지의 거리로 명시했을 때, 다음의 두 식으로 정리된다(식 (1)과 (2)).  

<p align="center"><img width="50%" src="https://user-images.githubusercontent.com/59362257/151526049-ea9181f5-34e0-4a21-b69f-a8563c282f26.png" /></p>

- 식 (1)은 D와 B의 비율은 f와 xL - xR의 비율과 같으므로 이것을 D에 대해 정리한 식이다.  
- 식 (2)는 D와 W의 비율은 f와 w의 비율과 같으므로 이것을 f에 대해 정리한 식이다.  
- 이 때, w는 CCD(Charge-Coupled Device)에 의한 목표물 이미지의 폭(mm), W는 CCD에 의하여 표시될 수 있는 목표물의 크기를 의미한다.


### 2. 4 Distance Reward

<p align="left"><img width="25%" src="https://user-images.githubusercontent.com/59362257/151532019-ba710b20-156a-4d13-a2cc-74aac69e3c78.png"/></p>

- Using Triangulation to measure the distance to the detected object.  
- Normalize the measurement distance to a value of 0 to 1 by divided by the limit distance.
- If no object is detected within range, return -1.





## 3. Train

### 3. 1 Set Learning Environment
- Place 244 object-shaped obstacles evenly.  
- Environment in the form of a 1km long straight track.
- Distributed Reinforcement Learning 3,500,000 steps.  

<p align="center"><img width="75%" src="https://user-images.githubusercontent.com/59362257/151547869-3ea16d7f-e140-471e-b167-91ee1cc2b887.png" /></p>


### 3. 2 Training Data

|Observe Status|Params|Description|
|:---|---|:---|
|Agent Position|3|드론의 위치정보|
|Agent Velocity|3|드론의 속도정보|
|Agent Angular Velocity|3|드론의 각속도 정보|
|Target Position|3|목표지점의 위치정보|
|Camera Observation|50|카메라 탐지물체 거리정보|

- 62개의 상태정보
- 3가지 행동(x, y, z축 이동)
- 상태정보(62개)를 통해 행동(3가지)을 결정


### 3. 3 PPO
- The machine learning of this project is based on PPO.
- 매 Iteration마다 N개의 Actor가 T개의 timestep만큼의 데이터를 모아 학습하는 방식
- NT개의 데이터를 통해 surrogate loss를 형성하고, minibatch SGD를 적용해 학습
- K epoch에 걸쳐 반복

<p align="center"><img width="75%" src="https://user-images.githubusercontent.com/59362257/151132334-e20f7d1c-2250-476c-9056-7b5a2d4d2667.png" /></p>

- Actor-Critic  
  Actor는 상태가 주어졌을 때 행동결정, Critic은 상태의 가치를 평가
- Surrogate Loss Function  
  대리손실함수, 손실함수가 경사하강법(SGD) 기반의 최적화 알고리즘 사용하는 것이 불가능할 때, 이를 대신하는 손실함수
- SGD(Stochastic Gradient Descent)  
  확률적 경사하강법, 일부 데이터의 모음(Mini-Batch)으로 경사하강법을 수행하는 것  
  다소 부정확할 수 있으나 빠른 계산 속도를 가짐  


### 3. 4 StereoFlight.yaml
- Make `yaml` file like below.

<p align="left"><img width="25%" src="https://user-images.githubusercontent.com/59362257/151126806-635b9693-aaba-4d9d-a3c8-f150f7ab6116.png"/></p>

- Key Parameters  

|Parameter Name|Description|
|:---|:---|
|batch_size|경사하강 1회 업데이트에 사용할 경험의 수|
|learning_rate|경사하강 학습의 정도|
|epsilon|이전 정책과 새 정책 사이의 비율 크기 제한|
|max_steps|학습할 총 step 수|


### 3. 3 Training
- After set learning environment, start machine learning with `Anaconda3` like below.  
- Start Training 3,500,000 Steps.  
```anaconda3
~/ml-agents> mlagents-learn config/ppo/StereoFlight.yaml --run-id=StereoFlight
```  
<p align="center"><img width="75%" src="Images/Anaconda_LidarFlight_001.PNG" /></p>
<p align="center"><img width="75%" src="Images/Anaconda_LidarFlight_002.PNG" /></p>
<p align="center"><img width="75%" src="Images/Anaconda_LidarFlight_003.PNG" /></p>
<p align="center"><img width="75%" src="Images/Anaconda_LidarFlight_004.PNG" /></p>

- In the image above, the LiDAR_Flight_Bak1.yaml file is recycled.  
- If you made StereoFlight.yaml file, you can use the above command.  


------------여기부터 수정필요
### 3. 4 Training Result

<p align="center"><img width="100%" src="https://user-images.githubusercontent.com/59362257/151128795-5142eab1-a63d-49a8-95ce-63a94f90699f.png" /></p>

1,500,000 ~ 2,000,000 Step부터 보상이 증가하지 않음  
- Cumulative Reward  
모든 에이전트에 대한 평균 누적 에피소드 보상
- Extrinsic Reward  
에피소드당 환경에서 받는 평균 누적 보상
- Extrinsic Value Estimate  
에이전트가 방문한 모든 상태에 대한 평균값 추정치


## 4. Run
Run 5 times of the model trained above, 100 times each for the following two environments.  

### 4. 1 TEST01
- 원기둥 형태의 장애물 20개를 배치한 1km 길이의 일자형 트랙  

<p align="center"><img width="75%" src="Images/Test_001.jpg" /></p>

- Tried 100 times.
- Achieved about 24s Average Time.
- Achieved 96% Accuracy.

### 4. 2 TEST02
- 사물 형태의 장애물 244개를 배치한 1km 길이의 일자형 트랙  

<p align="center"><img width="75%" src="Images/Test_002.png" /></p>

- Tried 100 times.
- Achieved about 24s Average Time.
- Achieved 86% Accuracy.

### 4. 3 Test Result
![image](https://user-images.githubusercontent.com/59362257/151134061-5691f52a-f670-4276-a49e-2001b75375f1.png)
![image](https://user-images.githubusercontent.com/59362257/151134068-dbd46353-d922-4a0b-8c5e-4f399fe240e1.png)

|Classification|Environment1|Environment2|
|---|---|---|
|Average Accuracy|94.00%|87.20%|
|Average Time|24.00s|24.15s|

- 환경에 따라 다소의 차이는 있으나, 대체로 목적지를 찾아가는 것을 확인
- 한 번의 학습으로 학습되지 않은 환경에서도 자율비행 가능

### 4. 4 Demo Simulation

<p align="center"><img width="75%" src="Images/Demo_001.png" /></p>
<p align="center"><img width="75%" src="Images/Demo_002.jpg" /></p>
<p align="center"><img width="75%" src="Images/Demo_003.jpg" /></p>

- Tried 100 times.
- Achieved about 11.5s Average Time.
- Achieved 89% Accuracy.


## 5. Results

- 정확도  
  학습되지 않은 환경에서도 높은 정확도를 보임  
- 속도  
  동일거리에 대해 항상 일정한 속도로 목표지점 도달  
- 목적 달성의 용이성  
  강화학습은 목적에 따라 적절한 보상설계를 통해 효율적으로 목적 달성 가능  
- 한 번의 학습으로 학습되지 않은 환경에서도 자율비행 가능  
  한정된 자원으로 여러 다른 문제 해결 가능  
