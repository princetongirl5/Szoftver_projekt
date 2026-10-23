import { TestBed } from '@angular/core/testing';

import { ThemeService } from './theme.service';

describe('ThemeService', () => {
  // The test environment has neither matchMedia nor localStorage.
  const mockOsDark = (dark: boolean) =>
    Object.defineProperty(window, 'matchMedia', {
      configurable: true,
      value: () => ({ matches: dark }),
    });

  beforeEach(() => {
    const store = new Map<string, string>();
    vi.stubGlobal('localStorage', {
      getItem: (key: string) => store.get(key) ?? null,
      setItem: (key: string, value: string) => store.set(key, value),
    });
  });

  afterEach(() => vi.unstubAllGlobals());

  it('follows the OS preference when nothing is saved', () => {
    mockOsDark(true);
    const service = TestBed.inject(ThemeService);
    TestBed.tick();

    expect(service.theme()).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
  });

  it('defaults to light when the OS has no dark preference', () => {
    mockOsDark(false);
    expect(TestBed.inject(ThemeService).theme()).toBe('light');
  });

  it('prefers the saved choice over the OS', () => {
    mockOsDark(true);
    localStorage.setItem('cardio-theme', 'light');
    expect(TestBed.inject(ThemeService).theme()).toBe('light');
  });

  it('toggle switches the theme and saves it', () => {
    mockOsDark(false);
    const service = TestBed.inject(ThemeService);

    service.toggle();

    expect(service.theme()).toBe('dark');
    expect(localStorage.getItem('cardio-theme')).toBe('dark');
  });
});
