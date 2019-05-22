import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { PerformerDialog } from '../performer-dialog/performer-dialog.component';
import { MatDialog } from '@angular/material';
import { PerformerService } from 'src/app/services/performer.service';

export interface Country {
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
  @Output() updateAlbumsEvent = new EventEmitter<Performer>();
  @Output() updatePerformersEvent = new EventEmitter();

  hover = false;

  constructor(public dialog: MatDialog, private performerService: PerformerService) { }

  ngOnInit() {
  }

  viewAlbums(): void {
    this.updateAlbumsEvent.next(this.performer);
  }

  edit(): void {
    const performerCopy = Object.assign({}, this.performer);

    const dialogRef = this.dialog.open(PerformerDialog, {
      width: '720px',
      data: performerCopy
    });

    dialogRef.afterClosed().subscribe(edited => {
      if (edited !== undefined) {
        edited.CountryId = edited.Country.Id;
        this.performerService.updatePerformer(this.performer.Id, edited)
          .subscribe(() => this.updatePerformersEvent.next());
      }
    });
  }

  delete(): void {
    this.performerService.deletePerformer(this.performer.Id)
      .subscribe(() => this.updatePerformersEvent.next());
  }
}
