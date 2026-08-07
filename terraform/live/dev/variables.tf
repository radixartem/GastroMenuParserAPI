# Обязательные секреты
variable "hcloud_token" {
  sensitive = true
}

variable "obj_access_key" {
  sensitive = true
  default   = null   # для бэкенда
}

variable "obj_secret_key" {
  sensitive = true
  default   = null
}

# Общие настройки
variable "location" {
  type = string
}

variable "ssh_key_name" {
  type = string
  description = "Имя существующего SSH-ключа в Hetzner (будет использован data)"
}

variable "project_name" {
  type = string
}

variable "environment" {
  type = string
}

# Карта серверов
variable "servers" {
  type = map(object({
    server_type   = string
    image         = string
    attach_volume = bool          # прикрепить том только к этому серверу
    prevent_destroy = bool        # защита от удаления для production
  }))
}

# Firewall – используем существующий
variable "firewall_id" {
  type    = number
  default = null
  description = "ID существующего файрвола (не создаём новый)"
}

# Private Network – опционально
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

# Для шаблонизации cloud-init
variable "deploy_user" {
  type    = string
  default = "deploy"
}
variable "github_user" {
  type    = string
  default = ""   # опционально: для добавления публичных ключей из GitHub
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