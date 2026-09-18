import openapiTS, { astToString } from 'openapi-typescript';
import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

const openApiUrl = process.env.OPENAPI_URL || 'http://localhost:5023/openapi.json';
const outputPath = path.resolve(__dirname, '../src/shared/api/generated/schema.d.ts');

async function generate() {
  console.log(`[OpenAPI Codegen] Conectando a ${openApiUrl}...`);

  try {
    const res = await fetch(openApiUrl, { signal: AbortSignal.timeout(5000) });
    if (!res.ok) {
      throw new Error(`HTTP ${res.status}: ${res.statusText}`);
    }

    const spec = await res.json();
    const ast = await openapiTS(spec);
    const output = astToString(ast);

    await fs.mkdir(path.dirname(outputPath), { recursive: true });
    await fs.writeFile(outputPath, output, 'utf-8');

    console.log(`[OpenAPI Codegen] Esquema TypeScript generado exitosamente en ${outputPath}`);
  } catch (error) {
    console.error(
      `[OpenAPI Codegen Error] No se pudo obtener la especificación OpenAPI desde '${openApiUrl}'.\n` +
        `Detalle: ${error instanceof Error ? error.message : String(error)}\n` +
        `El esquema actual en ${outputPath} no ha sido modificado.`
    );
    process.exit(1);
  }
}

generate();
