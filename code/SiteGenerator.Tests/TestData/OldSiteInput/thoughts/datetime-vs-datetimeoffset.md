# Datetime vs datetimeoffset

Quick notes: datetimeoffset is fine since you can query underlying UTC. Still it takes 2 bytes to store the offset on all rows vs in name/meta data so if all rows are written from the same time zone it's a bit of a waste.

See more at [sqlonice](https://sqlonice.com/the-road-to-utc-datetimeoffset/)
