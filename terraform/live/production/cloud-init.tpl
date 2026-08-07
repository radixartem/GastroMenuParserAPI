#cloud-config
package_update: true
packages:
  - git
  - ansible
  - python3-pip

users:
  - name: ${deploy_user}
    sudo: ALL=(ALL) NOPASSWD:ALL
    shell: /bin/bash

runcmd:
  # Clone a repository with Ansible playbooks
  - git clone --branch ${ansible_branch} ${ansible_repo} /tmp/gastro-ops
  # Launching the playbook
  - ansible-playbook -i /tmp/gastro-ops/ops/ansible/inventory/hosts.yml /tmp/gastro-ops/ops/ansible/playbooks/bootstrap.yml --extra-vars "deploy_user=${deploy_user}"
  # Delete the temporary folder
  - rm -rf /tmp/gastro-ops