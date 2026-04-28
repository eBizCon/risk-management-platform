#!/bin/bash

az account set --subscription "6c27b627-042a-40d3-9d98-f2456e3e534b"

az ad sp create-for-rbac \
  --name "sp-cli-reader-workshop" \
  --role "Reader" \
  --scopes "/subscriptions/6c27b627-042a-40d3-9d98-f2456e3e534b"