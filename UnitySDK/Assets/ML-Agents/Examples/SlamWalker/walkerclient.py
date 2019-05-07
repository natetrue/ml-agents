env_name = None  # Name of the Unity environment binary to launch
train_mode = True  # Whether to run the environment in training or inference mode

import numpy as np
import sys
import cv2

from mlagents.envs import UnityEnvironment


def depth_rgb_to_float(depth_rgb_img):
    depth_img = depth_rgb_img[:,:,0] * 16777216 + depth_rgb_img[:,:,1] * 65536 + depth_rgb_img[:,:,2] * 256
    depth_img = (depth_img / 16777216.0) * (100 - 0.3) + 0.3
    return depth_img

def walking_iterator():
    env = UnityEnvironment(file_name=env_name)

    # Set the default brain to work with
    default_brain = env.brain_names[0]
    brain = env.brains[default_brain]

    # Reset the environment
    env_info = env.reset(train_mode=train_mode)[default_brain]

    # Examine the state space for the default brain
    print("Agent vector observations look like: \n{}".format(env_info.vector_observations[0]))

    # Examine the observation space for the default brain
    print("Agent visual observations look like:")
    for i, vo in enumerate(env_info.visual_observations):
        print("Visual observation", i, ":", vo[0].shape)

    turning_sign = 1
    while True:
        # Interpret and yield sensory input
        rgb_image = env_info.visual_observations[0][0]
        depth_image = depth_rgb_to_float(env_info.visual_observations[1][0])
        pose = env_info.vector_observations[0][:4]
        forward_clear_dist = env_info.vector_observations[0][4]

        yield {
            'image': rgb_image,
            'depth': np.clip(depth_image * 1000, 0, 65535).astype(np.uint16),
            'pose': pose
        }

        # Decide on actions
        # First action dim is forward motion, second is rotation
        actions = np.zeros([len(env_info.agents), brain.vector_action_space_size[0]], np.float32)

        if forward_clear_dist > 3.0:
            turning_sign = -1 * turning_sign

        if forward_clear_dist > 1.0:
            # Forward is clear, go forward
            actions[0,0] = np.random.uniform(0.05, 0.5)
            actions[0,1] = np.random.uniform(0.0, 0.01) * turning_sign
        elif forward_clear_dist < 0.1:
            # Back up!
            actions[0,0] = np.random.uniform(-0.05, -0.5)
        else:
            # Just a little distance. Turn
            actions[0,1] = np.random.uniform(0.01, 0.05) * turning_sign

        env_info = env.step(actions)[default_brain]
        if env_info.local_done[0]:
            env_info = env.reset(train_mode=train_mode)
        if type(env_info) is dict:
            # This happens sometimes, not sure why
            env_info = env_info['SlamWalkerLearning']

if __name__ == "__main__":
    for x in walking_iterator():
        print(x['pose'])
        cv2.imshow('image', x['image'])
        cv2.imshow('depth', x['depth'])
        cv2.waitKey(10)

env.close()
'''
for episode in range(10):
    env_info = env.reset(train_mode=train_mode)[default_brain]
    done = False
    episode_rewards = 0
    while not done:
        action_size = brain.vector_action_space_size
        if brain.vector_action_space_type == 'continuous':
            env_info = env.step(np.random.randn(len(env_info.agents), 
                                                action_size[0]))[default_brain]
        else:
            action = np.column_stack([np.random.randint(0, action_size[i], size=(len(env_info.agents))) for i in range(len(action_size))])
            env_info = env.step(action)[default_brain]
        episode_rewards += env_info.rewards[0]
        done = env_info.local_done[0]
    print("Total reward this episode: {}".format(episode_rewards))

env.close()
'''