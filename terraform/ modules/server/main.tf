resource "hcloud_server" "this" {
  name        = var.server_name
  server_type = var.server_type
  image       = var.image
  location    = var.location
  ssh_keys    = var.ssh_key_ids
  user_data   = var.user_data

  labels = merge(
    var.labels,
    {
      "managed-by" = "terraform"
      "environment" = var.environment
    }
  )

  dynamic "network" {
    for_each = var.network_id != null ? [1] : []
    content {
      network_id = var.network_id
    }
  }

  firewall_ids = var.firewall_ids

  lifecycle {
    # Protection against accidental deletion of a production server
    prevent_destroy = var.prevent_destroy
  }
}

resource "hcloud_server_volume_attachment" "this" {
  count     = var.volume_id != null ? 1 : 0
  server_id = hcloud_server.this.id
  volume_id = var.volume_id
}