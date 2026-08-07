ENV ?= production
TF_DIR = live/$(ENV)

.PHONY: init plan apply destroy import fmt validate lint

init:
	cd $(TF_DIR) && terraform init

plan:
	cd $(TF_DIR) && terraform plan

apply:
	cd $(TF_DIR) && terraform apply

destroy:
	cd $(TF_DIR) && terraform destroy

import:
	cd $(TF_DIR) && terraform import $(ARGS)

fmt:
	cd $(TF_DIR) && terraform fmt -recursive

validate:
	cd $(TF_DIR) && terraform validate

lint:
	cd $(TF_DIR) && tflint --config=../../.tflint.hcl

# Пример: make import ARGS="module.servers[\"production\"].hcloud_server.this 12345"