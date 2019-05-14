import { Component, Input, OnInit } from '@angular/core';

interface Country {
  Id: number;
  Name: string;
}

export interface Performer {
  Id: number;
  Name: string;
  Info: string;
  Country: Country;
  Link: string;
  CountryLink: string;
}

@Component({
  selector: 'app-performer',
  templateUrl: './performer.component.html',
  styleUrls: ['./performer.component.css']
})
export class PerformerComponent implements OnInit {

  @Input() performer: Performer;

  hover = false;

  constructor() { }

  ngOnInit() {
  }

  viewAlbums() {

  }

  edit() {

  }
}
