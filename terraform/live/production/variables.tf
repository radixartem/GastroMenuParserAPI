# Mandatory secrets
variable "hcloud_token" {
  sensitive = true
}

variable "obj_access_key" {
  sensitive = true
  default   = null   # for the backend
}

variable "obj_secret_key" {
  sensitive = true
  default   = null
}

# General settings
variable "location" {
  type = string
}

variable "ssh_key_name" {
  type = string
  description = "Name of an existing SSH key in Hetzner (data will be used)"
}

variable "project_name" {
  type = string
}

variable "environment" {
  type = string
}

# Server map
variable "servers" {
  type = map(object({
    server_type   = string
    image         = string
    attach_volume = bool          # attach the volume to this server only
    prevent_destroy = bool        # deletion protection for production
  }))
}

# Firewall – use the existing one
variable "firewall_id" {
  type    = number
  default = null
  description = "ID существующего файрвола (не создаём новый)"
}

# Private Network – optional
variable "create_network" {
  type    = bool
  default = false
}
variable "network_name" {
  type    = string
  default = "gastro-network"
}
variable "network_ip_range" {
  type    = string
  default = "10.0.0.0/16"
}

# Volume
variable "create_volume" {
  type    = bool
  default = false
}
variable "volume_name" {
  type    = string
  default = "postgres-data"
}
variable "volume_size_gb" {
  type    = number
  default = 10
}

# For cloud-init templating
variable "deploy_user" {
  type    = string
  default = "deploy"
}
variable "github_user" {
  type    = string
  default = ""   # optional: to add public keys from GitHub
}
# Ansible репозиторий
variable "ansible_repo" {
  type    = string
  default = "https://github.com/your-org/gastro-api.git"
}
variable "ansible_branch" {
  type    = string
  default = "main"
}