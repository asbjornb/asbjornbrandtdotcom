# Home lab

## Current setup

* NUC running Linux Mint Cinnamon

## Services

* Navidrome - music streaming
* Plex - TV shows (planned, if Navidrome works well)

## Navidrome + Tailscale setup

Running Navidrome on the NUC with Tailscale for secure remote access.

### Mobile access with Symfonium (Android)

1. Install Tailscale on phone and connect to tailnet
2. In Symfonium, add a new server:
   - Server type: Open
   - Address: `http://{tailscale-ip}:{navidrome-port}`
   - Username and password from Navidrome

## Purposes

* [[backup]] of photos and documents
* Lower consumption from [[pc-setup]] by replacing some uses with NUC
* Try out some things with a SBC. Probably [[retropie]].

## More information

<https://www.groovypost.com/howto/enable-wake-on-lan-windows-10/> Wake on lan
Also check saved comments on reddit - saved some great security posts for homelabbing.
