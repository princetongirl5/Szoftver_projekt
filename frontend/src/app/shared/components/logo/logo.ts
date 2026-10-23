import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-logo',
  imports: [RouterLink],
  template: `<a class="cx-brand" routerLink="/">c<span class="cx-brand__hash">#</span>ardio</a>`,
  host: { style: 'display: contents' },
})
export class Logo {}
