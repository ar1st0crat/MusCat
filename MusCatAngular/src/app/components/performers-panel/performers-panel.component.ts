import { Component, OnInit, ViewChild } from '@angular/core';
import { MatPaginator, _MatChipListMixinBase } from '@angular/material';
import { animate, state, style, transition, trigger } from '@angular/animations';
import { PerformerService } from 'src/app/services/performer.service';
import { Performer } from '../performer/performer.component';
import { Album, AlbumsPanelComponent } from '../albums-panel/albums-panel.component';

@Component({
  selector: 'app-performers-panel',
  templateUrl: './performers-panel.component.html',
  styleUrls: ['./performers-panel.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({height: '0px', minHeight: '0', display: 'none'})),
      state('expanded', style({height: '*'})),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)')),
    ]),
  ]
})
export class PerformersPanelComponent implements OnInit {

  @ViewChild(MatPaginator) paginator: MatPaginator;

  pageCount = 0;
  pageSize = 8;

  performers: Performer[];
  performer: Performer;
  albums: Album[];
  selectedAlbum: Album;

  codes = {
    England: 'GB',
    Scotland: 'GB',
    Wales: 'GB',
    USA: 'US',
    Germany: 'DE',
    Sweden: 'SE',
    Netherlands: 'NL',
    Italy: 'IT',
    France: 'FR',
    Russia: 'RU',
    Ukraine: 'UA',
    Japan: 'JP',
    Canada: 'CA',
    Brazil: 'BR',
    Belgium: 'BE',
    Spain: 'ES',
    Argentina: 'AR',
    Australia: 'AU',
    Denmark: 'DK',
    Switzerland: 'CH',
    Finland: 'FI',
    Ireland: 'IE',
    Mexico: 'MX',
    Norway: 'NO',
    Poland: 'PL',
    Greece: 'GR',
    Venezuela: 'VE',
    Hungary: 'HU',
    Ghana: 'GH',
    Iceland: 'IS',
    International: 'EU',
    Unknown: 'AQ'
  };

  constructor(private performerService: PerformerService) { }

  updatePerformers() {
    this.performerService.getPerformers()
      .subscribe(p => {
        this.pageCount = p.TotalItems;
        this.performers = p.Items;
        this.performers.forEach(d => {
          d.Link = `${this.performerService.uri}/performers/${d.Id}/photo`;
          if (d.Country === undefined) { d.Country = { Id: 0, Name: 'Unknown' }; }
          d.CountryLink = `https://www.countryflags.io/${this.codes[d.Country.Name]}/shiny/48.png`;
        });
      });
  }

  updateAlbums(performer: Performer) {
    this.performerService.getAlbums(performer.Id)
    .subscribe(a => {
      this.performer = performer;
      this.selectedAlbum = { Id: 0 };
      this.albums = a;
      this.albums.forEach(d => {
        d.Link = `${this.performerService.uri}/albums/${d.Id}/cover`;
      });
    });
  }

  ngOnInit() {
    this.paginator.page.subscribe((event) => {
      this.performerService.page = event.pageIndex;
      this.performerService.size = event.pageSize;
      this.updatePerformers();
    });
    this.updatePerformers();
  }
}
