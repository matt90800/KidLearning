import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';

import { SpeechService } from '../../services/speech.service';

interface Letter {
  letter: string;
  lowercase: string;

  // Real phoneme used for reading.
  phoneme: string;

  word: string;
  emoji: string;
}

@Component({
  selector: 'app-letter-game',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule
  ],
  templateUrl: './letter-game.component.html',
  styleUrl: './letter-game.component.css'
})
export class LetterGameComponent implements OnInit {

  readonly letters: Letter[] = [
    {
      letter: 'A',
      lowercase: 'a',
      phoneme: 'a',
      word: 'avion',
      emoji: '✈️'
    },
    {
      letter: 'M',
      lowercase: 'm',
      phoneme: 'm',
      word: 'maman',
      emoji: '👩'
    },
    {
      letter: 'P',
      lowercase: 'p',
      phoneme: 'p',
      word: 'papa',
      emoji: '👨'
    },
    {
      letter: 'S',
      lowercase: 's',
      phoneme: 's',
      word: 'soleil',
      emoji: '☀️'
    },
    {
      letter: 'L',
      lowercase: 'l',
      phoneme: 'l',
      word: 'lune',
      emoji: '🌙'
    },
    {
      letter: 'R',
      lowercase: 'r',
      phoneme: 'ʁ',
      word: 'robot',
      emoji: '🤖'
    }
  ];

  currentLetter!: Letter;
  choices: Letter[] = [];

  score = 0;
  streak = 0;

  answered = false;
  correct = false;

  private currentIndex = 0;

  constructor(
    private readonly speechService: SpeechService
  ) {}

  ngOnInit(): void {
    this.startGame();
  }

  startGame(): void {
    this.score = 0;
    this.streak = 0;
    this.currentIndex = 0;

    this.nextQuestion();
  }

  nextQuestion(): void {
    this.answered = false;
    this.correct = false;

    this.currentLetter =
      this.letters[this.currentIndex % this.letters.length];

    this.choices = this.createChoices(this.currentLetter);
  }

  async selectLetter(letter: Letter): Promise<void> {
    if (this.answered) {
      return;
    }

    this.answered = true;
    this.correct = letter.letter === this.currentLetter.letter;

    if (this.correct) {
      this.score++;
      this.streak++;

      await this.playCurrentPhoneme();
    } else {
      this.streak = 0;
    }
  }

  async playCurrentPhoneme(): Promise<void> {
    await this.speechService.playPhoneme(
      this.currentLetter.phoneme
    );
  }

  continueGame(): void {
    this.currentIndex++;
    this.nextQuestion();
  }

  get progress(): number {
    return ((this.currentIndex % this.letters.length) + 1)
      / this.letters.length * 100;
  }

  private createChoices(correct: Letter): Letter[] {
    const others = this.letters
      .filter(letter => letter.letter !== correct.letter)
      .sort(() => Math.random() - 0.5)
      .slice(0, 2);

    return [correct, ...others]
      .sort(() => Math.random() - 0.5);
  }
}
