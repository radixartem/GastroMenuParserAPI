output "server_ips" {
  value = { for k, v in module.servers : k => v.ipv4_address }
}

output "server_ids" {
  value = { for k, v in module.servers : k => v.server_id }
}

output "firewall_id" {
  value = local.firewall_id
}

output "network_id" {
  value = local.network_id
}

output "volume_id" {
  value = local.volume_id
  sensitive = true
}