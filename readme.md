# How can a drone autonomously follow a person by using reinforcement learning?

This project focuses on simulating a drone in Unity using ML-Agents for AI training. The drone is trained to recognize people via a Ray Perception Lazer in Unity, fine-tuning its behavior for practical applications. This README provides an overview of the project and directs users to further resources.

---

## Project Overview

The research involves creating a fully simulated environment where a drone can:

- Detect people using Unity ML-Agents.
- Navigate in a virtual space.

This eliminates the need for a real-world setup, making it a cost-effective and safe alternative for training and testing.

---

## Features

- **Drone Simulation**: Somwehat accurate modeling of drone physics and behavior.
- **Unity ML-Agents Integration**: Seamless connection between Unity's environment and AI training models.
- **People Detection**: Using a virtual lazer.

---

## Prerequisites

To work with this project, ensure you have the following:

- Unity Editor (Version 2023.2 or newer recommended)
- Unity ML-Agents Toolkit
- Python (>= 3.10.1, <=3.10.12)

---

## Installation and User Guide

For detailed instructions on setting up the project and using the simulation:

1. Navigate to the `/Documents` folder in this repository.
2. Open the following files:
   - **Installation Guide**: `/Documents/InstallatieHandleiding-OlivierCardoen.pdf`
   - **User Guide**: `/Documents/GebruikersHandleiding-OlivierCardoen.pdf`

---

## Project Structure

The important files in this project are all located in the `/Project` folder. 
To use the lastest project version, navigate to the following directory:

```
├── Assets/
    ├── ML-Agents/ 
        ├── Drone-NoTimer/
```

The structure looks as the following:

```

├── Models/               # Pre-trained AI models and configurations
├── Prefabs/              # Environments made to be copied to accelerate training. Or to be used as demo purposes
├── Scenes/               # The environment the player / user will see during training or demos.
├── Scripts/              # Unity C# scripts
├── Materials/            # Materials/Colors used in various ingame objects
```

---

## Contributors

- **Olivier Cardoen** - IoT Engineer & Researcher

---

## License

This project is licensed under the Apache License. See the LICENSE file for more details.
Please also read the NOTICE file for further explenation.

---

For further questions, feel free to reach out or open an issue in this repository.