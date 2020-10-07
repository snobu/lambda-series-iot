#!/bin/sh
az iot device c2d-message send -d x509-selfsigned-device -n poorlyfundedskynet --data 'Hello from Azure IoT Hub, X.509 authenticated device.'
