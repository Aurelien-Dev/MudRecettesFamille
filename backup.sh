#!/bin/bash
docker exec recettesfamille.data pg_dump -U pguser -d recettesfamilledb -F c -f /home/backup.sql
docker cp recettesfamille.data:/home/backup.sql ./backup.sql
docker cp ./backup.sql recettesfamille.client:/app/wwwroot/backups/backup.sql