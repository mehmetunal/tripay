#!/usr/bin/env node
'use strict';

/**
 * Kaynak: wwwroot/css (degismez)
 * Cikti: wwwroot/css-built/** (ayni alt yol + .min.css) — Trimango.Web ile ayni mantik
 */
const fs = require('fs');
const path = require('path');

const projectRoot = path.join(__dirname, '..');
const wwwroot = path.join(projectRoot, 'wwwroot');
const outRoot = path.join(wwwroot, 'css-built');

function collectCssFiles(dir, outList) {
    if (!fs.existsSync(dir)) {
        return;
    }
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
        const full = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            collectCssFiles(full, outList);
            continue;
        }
        if (!entry.isFile() || !entry.name.endsWith('.css') || entry.name.endsWith('.min.css')) {
            continue;
        }
        outList.push(full);
    }
}

function toDestPath(absSourceFile) {
    const relFromWwwroot = path.relative(wwwroot, absSourceFile);
    const parts = relFromWwwroot.split(path.sep);
    if (parts.length < 2 || parts[0].toLowerCase() !== 'css') {
        return null;
    }
    const dir = path.dirname(relFromWwwroot);
    const name = path.parse(absSourceFile).name;
    return path.join(outRoot, dir, `${name}.min.css`);
}

function minifyCss(content) {
    return content
        .replace(/\/\*[\s\S]*?\*\//g, '')
        .replace(/\s+/g, ' ')
        .replace(/\s*([{}:;,>+~])\s*/g, '$1')
        .replace(/;}/g, '}')
        .trim();
}

function minifyOne(srcPath, destPath) {
    const source = fs.readFileSync(srcPath, 'utf8');
    const minified = minifyCss(source);
    fs.mkdirSync(path.dirname(destPath), { recursive: true });
    fs.writeFileSync(destPath, minified, 'utf8');
}

function main() {
    const cssRoot = path.join(wwwroot, 'css');
    if (!fs.existsSync(cssRoot)) {
        console.warn('Uyari: wwwroot/css yok, css-built atlandi.');
        return;
    }

    if (fs.existsSync(outRoot)) {
        fs.rmSync(outRoot, { recursive: true, force: true });
    }
    fs.mkdirSync(outRoot, { recursive: true });

    const files = [];
    collectCssFiles(cssRoot, files);

    if (files.length === 0) {
        console.warn('Uyari: minify edilecek CSS bulunamadi.');
        return;
    }

    let ok = 0;
    let fail = 0;
    for (const srcPath of files) {
        const destPath = toDestPath(srcPath);
        if (!destPath) {
            continue;
        }
        try {
            minifyOne(srcPath, destPath);
            ok++;
        } catch (e) {
            fail++;
            console.error('Hata:', srcPath, e && e.message ? e.message : e);
        }
    }

    console.log(`css-built tamamlandi. Basarili: ${ok}, Hatali: ${fail}, Hedef: ${outRoot}`);
    if (fail > 0) {
        process.exit(1);
    }
}

main();
