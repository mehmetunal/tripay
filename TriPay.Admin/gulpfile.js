/**
 * TriPay.Admin — Gulp (Trimango.Web ile uyumlu)
 *
 * - Tailwind derlemesi → wwwroot/css/admin.css (kaynak, minify degil)
 * - JS minify → scripts/build-admin-js-built.js → wwwroot/js-built/**
 * - CSS minify → scripts/build-admin-css-built.js → wwwroot/css-built/**
 */
const gulp = require('gulp');
const postcss = require('gulp-postcss');
const tailwindcss = require('@tailwindcss/postcss');
const rename = require('gulp-rename');
const { deleteAsync } = require('del');
const { execAsync } = require('./scripts/exec-async');

const paths = {
  css: {
    src: 'Styles/app.css',
    dest: 'wwwroot/css',
    out: 'admin.css'
  }
};

function clean() {
  return deleteAsync([
    `${paths.css.dest}/admin.css`,
    `${paths.css.dest}/admin.min.css`,
    'wwwroot/js-built/**',
    'wwwroot/css-built/**'
  ]);
}

/** Tailwind ciktisi — yalnizca wwwroot/css (minify css-built scriptinde) */
function styles() {
  return gulp
    .src(paths.css.src)
    .pipe(postcss([tailwindcss()]))
    .pipe(rename('admin.css'))
    .pipe(gulp.dest(paths.css.dest));
}

async function scripts() {
  await execAsync('node ./scripts/build-admin-js-built.js', { cwd: __dirname });
}

async function cssBuilt() {
  await execAsync('node ./scripts/build-admin-css-built.js', { cwd: __dirname });
}

function watch() {
  gulp.watch([paths.css.src, 'Views/**/*.cshtml'], gulp.series(styles, cssBuilt));
  gulp.watch('wwwroot/js/**/*.js', scripts);
}

const build = gulp.series(clean, styles, gulp.parallel(scripts, cssBuilt));

exports.clean = clean;
exports.styles = styles;
exports.scripts = scripts;
exports.cssBuilt = cssBuilt;
exports.watch = gulp.series(build, watch);
exports.build = build;
exports.default = build;
