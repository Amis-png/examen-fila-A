#!/usr/bin/env bash
# Sube un proyecto de Unity a GitHub.
# Uso: copia este archivo dentro de la carpeta del proyecto (D:\examen-fila-A)
#      y en Git Bash ejecuta:  bash subir_a_github.sh

USUARIO="Amis-png"
REPO="examen-fila-A"

set -e
cd "$(dirname "$0")"

if [ ! -d "Assets" ] || [ ! -d "ProjectSettings" ]; then
  echo "ERROR: aqui no hay un proyecto de Unity (faltan Assets/ProjectSettings)."
  echo "Pon este archivo dentro de la carpeta del proyecto y vuelve a ejecutarlo."
  exit 1
fi

# Nombre y correo (solo si no estan configurados)
if [ -z "$(git config --global user.name)" ]; then
  read -p "Tu nombre para Git: " NOMBRE
  git config --global user.name "$NOMBRE"
fi
if [ -z "$(git config --global user.email)" ]; then
  read -p "Tu correo de GitHub: " CORREO
  git config --global user.email "$CORREO"
fi

# Repositorio local
if [ ! -d ".git" ]; then
  git init -b main
fi

# .gitignore para Unity
cat > .gitignore <<'EOF'
# Carpetas que Unity genera solo
/[Ll]ibrary/
/[Tt]emp/
/[Oo]bj/
/[Bb]uild/
/[Bb]uilds/
/[Ll]ogs/
/[Uu]ser[Ss]ettings/
/[Mm]emoryCaptures/
/[Rr]ecordings/

# Asset Store
/[Aa]ssets/AssetStoreTools*
/[Aa]ssets/Plugins/Editor/JetBrains*

# Visual Studio / Rider
.vs/
.vscode/
.idea/
.gradle/
ExportedObj/
.consulo/
*.csproj
*.unityproj
*.sln
*.suo
*.tmp
*.user
*.userprefs
*.pidb
*.booproj
*.svd
*.pdb
*.mdb
*.opendb
*.VC.db
*.pidb.meta
*.pdb.meta
*.mdb.meta

# Crash reports y builds
sysinfo.txt
*.apk
*.aab
*.unitypackage
*.app
crashlytics-build.properties
/[Aa]ssets/[Aa]ddressable[Aa]ssets[Dd]ata/*/*.bin*
/[Aa]ssets/[Ss]treamingAssets/aa.meta
/[Aa]ssets/[Ss]treamingAssets/aa/*

# Sistema
.DS_Store
Thumbs.db
EOF

# Este script no se sube
echo "subir_a_github.sh" >> .gitignore

git add .
git commit -m "Proyecto inicial $REPO" || echo "(No habia cambios nuevos para guardar)"

URL="https://github.com/$USUARIO/$REPO.git"
if git remote | grep -q origin; then
  git remote set-url origin "$URL"
else
  git remote add origin "$URL"
fi

echo ""
echo "Subiendo a $URL ..."
echo "(Si se abre una ventana, inicia sesion en GitHub con el navegador)"
if git push -u origin main; then
  echo ""
  echo "LISTO. Abre: https://github.com/$USUARIO/$REPO"
else
  echo ""
  echo "No se pudo subir. Revisa que creaste el repositorio '$REPO' en GitHub"
  echo "(boton + > New repository, sin README ni .gitignore) y vuelve a ejecutar:"
  echo "  bash subir_a_github.sh"
fi
