resource "hcloud_network" "this" {
  name     = var.network_name
  ip_range = var.ip_range

  lifecycle {
    prevent_destroy = true
  }
}