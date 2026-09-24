import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SpeechService {
  private readonly apiUrl = '/api/speech';

  private readonly audioCache = new Map<string, string>();

  constructor(private readonly http: HttpClient) { }

  async playPhoneme(phoneme: string): Promise<void> {
    await this.play(
      `${this.apiUrl}/phoneme/${encodeURIComponent(phoneme)}`
    );
  }

  async playSyllable(syllable: string): Promise<void> {
    await this.play(
      `${this.apiUrl}/syllable/${encodeURIComponent(syllable)}`
    );
  }

  private async play(url: string): Promise<void> {
    let audioUrl = this.audioCache.get(url);

    if (!audioUrl) {
      const blob = await firstValueFrom(
        this.http.get(url, {
          responseType: 'blob'
        })
      );

      audioUrl = URL.createObjectURL(blob);
      this.audioCache.set(url, audioUrl);
    }

    const audio = new Audio(audioUrl);

    await audio.play();
  }
}
