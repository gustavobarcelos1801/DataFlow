const TEMPO_MINIMO_MS = 650;
const TEMPO_MAXIMO_MS = 1700;

export function sleep(ms: number): Promise<void> {
  return new Promise((resolve) => {
    setTimeout(resolve, ms);
  });
}

export function tempoAleatorioValidacao(): number {
  const intervalo = TEMPO_MAXIMO_MS - TEMPO_MINIMO_MS;
  return Math.round(TEMPO_MINIMO_MS + Math.random() * intervalo);
}