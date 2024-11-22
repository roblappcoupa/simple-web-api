from locust import HttpUser, task, between
import random
import uuid
from datetime import datetime, timedelta

class UserBehavior(HttpUser):
    # Wait time between tasks to simulate real user behavior
    wait_time = between(1, 3)

