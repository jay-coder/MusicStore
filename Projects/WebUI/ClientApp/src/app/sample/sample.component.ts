import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Component({
  selector: 'sample',
  templateUrl: './sample.component.html'
})
export class SampleComponent {
  public values: string[];

  constructor(http: HttpClient) {
    var valuesApiUrl = environment.apiUrl + 'values';
    http.get<string[]>(valuesApiUrl).subscribe(result => {
      this.values = result;
    }, error => console.error(error));
  }
}
