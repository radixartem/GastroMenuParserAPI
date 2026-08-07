variable "firewall_name" {
  type = string
}

variable "extra_rules" {
  type = list(object({
    direction  = string
    protocol   = string
    port       = string
    source_ips = list(string)
  }))
  default = []
}