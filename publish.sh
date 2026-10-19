#!/usr/bin/env bash
# Membuat paket yang bisa dibagikan, dari Mac:
#   dist/CompanyTrack.exe                  Windows 64-bit (satu file)
#   dist/CompanyTrack-mac-arm64.zip        macOS Apple Silicon (CompanyTrack.app)
#   dist/CompanyTrack-mac-x64.zip          macOS Intel (CompanyTrack.app)
set -euo pipefail
cd "$(dirname "$0")"

PROJECT=src/CompanyTrack/CompanyTrack.csproj
DIST=dist
VERSION="1.0.0"
rm -rf "$DIST" obj-publish
mkdir -p "$DIST"

echo "==> Windows x64"
dotnet publish "$PROJECT" -c Release -r win-x64 --self-contained true \
  -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true \
  -p:DebugType=none -p:DebugSymbols=false -o obj-publish/win-x64
cp obj-publish/win-x64/CompanyTrack.exe "$DIST/CompanyTrack.exe"

make_app() {
  local rid="$1" out="$2"
  echo "==> macOS $rid"
  dotnet publish "$PROJECT" -c Release -r "$rid" --self-contained true \
    -p:DebugType=none -p:DebugSymbols=false -o "obj-publish/$rid"
  local app="obj-publish/$rid-app/CompanyTrack.app"
  mkdir -p "$app/Contents/MacOS" "$app/Contents/Resources"
  cp -R "obj-publish/$rid/." "$app/Contents/MacOS/"
  cat > "$app/Contents/Info.plist" <<PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleName</key><string>CompanyTrack</string>
  <key>CFBundleDisplayName</key><string>CompanyTrack</string>
  <key>CFBundleIdentifier</key><string>id.ac.its.hmtc.companytrack</string>
  <key>CFBundleVersion</key><string>$VERSION</string>
  <key>CFBundleShortVersionString</key><string>$VERSION</string>
  <key>CFBundleExecutable</key><string>CompanyTrack</string>
  <key>CFBundlePackageType</key><string>APPL</string>
  <key>LSMinimumSystemVersion</key><string>11.0</string>
  <key>NSHighResolutionCapable</key><true/>
</dict>
</plist>
PLIST
  chmod +x "$app/Contents/MacOS/CompanyTrack"
  # Tanda tangan ad-hoc (tanpa Apple Developer ID); wajib agar binary arm64 mau berjalan.
  codesign --force --deep --sign - "$app" 2>/dev/null || echo "   (codesign dilewati)"
  ditto -c -k --keepParent "$app" "$DIST/$out"
}

make_app osx-arm64 CompanyTrack-mac-arm64.zip
make_app osx-x64   CompanyTrack-mac-x64.zip

rm -rf obj-publish
echo; ls -lh "$DIST"
