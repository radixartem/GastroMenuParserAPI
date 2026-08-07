# Obtaining an existing SSH key
data "hcloud_ssh_key" "main" {
  name = var.ssh_key_name
}

# We use an existing firewall (ID is passed in a variable)
locals {
  firewall_id = var.firewall_id
  network_id  = var.create_network ? hcloud_network.this[0].id : null
  volume_id   = var.create_volume ? hcloud_volume.this[0].id : null
}

# We create a network if necessary
resource "hcloud_network" "this" {
  count    = var.create_network ? 1 : 0
  name     = var.network_name
  ip_range = var.network_ip_range
}

# Create a volume if necessary
resource "hcloud_volume" "this" {
  count    = var.create_volume ? 1 : 0
  name     = var.volume_name
  size     = var.volume_size_gb
  location = var.location
}

# cloud-init template
locals {
  cloud_init = templatefile("${path.module}/cloud-init.tpl", {
    deploy_user    = var.deploy_user
    github_user    = var.github_user
    ansible_repo   = var.ansible_repo
    ansible_branch = var.ansible_branch
  })
}

# Server creation
module "servers" {
  source = "../../modules/server"
  for_each = var.servers

  server_name   = each.key
  server_type   = each.value.server_type
  image         = each.value.image
  location      = var.location
  ssh_key_ids   = [data.hcloud_ssh_key.main.id]
  user_data     = local.cloud_init
  environment   = var.environment
  labels        = { project = var.project_name }
  network_id    = local.network_id
  firewall_ids  = local.firewall_id != null ? [local.firewall_id] : []
  volume_id     = each.value.attach_volume ? local.volume_id : null
  prevent_destroy = each.value.prevent_destroy
}