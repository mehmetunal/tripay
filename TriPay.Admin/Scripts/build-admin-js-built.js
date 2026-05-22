#!/usr/bin/env node
'use strict';

/**
 * Kaynak: wwwroot/js (degismez)
 * Cikti: wwwroot/js-built/** (ayni alt yol + .min.js, Terser) — Trimango.Web ile ayni mantik
 */
const fs = require('fs');
const path = require('path');
const { minify } = require('terser');

const projectRoot = path.join(__dirname, '..');
const wwwroot = path.join(projectRoot, 'wwwroot');
const outRoot = path.join(wwwroot, 'js-built');

const sourceRoots = [{ abs: path.join(wwwroot, 'js'), label: 'js' }];

function collectJsFiles(dir, outList) {
    if (!fs.existsSync(dir)) {
        return;
    }
    for (const entry of fs.readdirSync(dir, { withFileTypes: true })) {
        const full = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            collectJsFiles(full, outList);
            continue;
        }
        if (!entry.isFile() || !entry.name.endsWith('.js') || entry.name.endsWith('.min.js')) {
            continue;
        }
        outList.push(full);
    }
}

function toDestPath(absSourceFile) {
    const relFromWwwroot = path.relative(wwwroot, absSourceFile);
    const parts = relFromWwwroot.split(path.sep);
    if (parts.length < 2 || parts[0].toLowerCase() !== 'js') {
        return null;
    }
    const dir = path.dirname(relFromWwwroot);
    const base = path.basename(absSourceFile, '.js');
    return path.join(outRoot, dir, `${base}.min.js`);
}

async function minifyOne(srcPath, destPath) {
    const source = fs.readFileSync(srcPath, 'utf8');
    const result = await minify(source, {
        compress: true,
        mangle: true,
        format: { comments: false }
    });
    if (!result || typeof result.code !== 'string') {
        throw new Error('Terser bos dondu: ' + srcPath);
    }
    fs.mkdirSync(path.dirname(destPath), { recursive: true });
    fs.writeFileSync(destPath, result.code, 'utf8');
}

async function main() {
    if (!fs.existsSync(wwwroot)) {
        console.error('wwwroot bulunamadi:', wwwroot);
        process.exit(1);
    }

    if (fs.existsSync(outRoot)) {
        fs.rmSync(outRoot, { recursive: true, force: true });
    }
    fs.mkdirSync(outRoot, { recursive: true });

    const files = [];
    for (const { abs } of sourceRoots) {
        collectJsFiles(abs, files);
    }

    if (files.length === 0) {
        console.warn('Uyari: minify edilecek JS bulunamadi.');
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
            await minifyOne(srcPath, destPath);
            ok++;
        } catch (e) {
            fail++;
            console.error('Hata:', srcPath, e && e.message ? e.message : e);
        }
    }

    console.log(`js-built tamamlandi. Basarili: ${ok}, Hatali: ${fail}, Hedef: ${outRoot}`);
    if (fail > 0) {
        process.exit(1);
    }
}

main().catch((e) => {
    console.error(e);
    process.exit(1);
});
