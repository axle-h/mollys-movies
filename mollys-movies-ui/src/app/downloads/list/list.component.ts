import { Component, OnInit } from '@angular/core';
import {
  LiveTransmissionStatus,
  MovieDownload,
  TransmissionService,
  MovieDownloadStatusCode,
  TorrentService,
} from '../../api';
import { map, mergeAll } from 'rxjs/operators';
import { from, of } from 'rxjs';

interface MovieDownloadAndStatus extends MovieDownload {
  liveStatus: LiveTransmissionStatus;
}

@Component({
  selector: 'mm-list',
  templateUrl: './list.component.html',
})
export class ListComponent implements OnInit {
  page?: number;
  limit?: number;
  count?: number;
  data?: MovieDownloadAndStatus[] | null;

  constructor(
    private readonly transmissionService: TransmissionService,
    private readonly movieTorrentService: TorrentService,
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.transmissionService
      .getAllDownloads({ page: this.page, limit: this.limit })
      .subscribe(x => {
        this.page = x.page;
        this.limit = x.limit;
        this.count = x.count;
        this.data = x.data as MovieDownloadAndStatus[];
        const started =
          this.data && this.data.filter(c => c.status === MovieDownloadStatusCode.Started);
        if (!started || started.length === 0) {
          return of(x);
        }
        const $liveStatuses = started.map(c =>
          this.movieTorrentService
            .getLiveTransmissionStatus({ imdbCode: c.imdbCode })
            .pipe(map(s => ({ context: c, status: s }))),
        );
        from($liveStatuses)
          .pipe(mergeAll())
          .subscribe(({ context, status }) => (context.liveStatus = status));
      });
  }

  getStatusIcon(context: MovieDownload) {
    switch (context.status) {
      case MovieDownloadStatusCode.Complete:
        return 'fas fa-check text-success';
      case MovieDownloadStatusCode.Downloaded:
        return 'fas fa-cog fa-spin text-secondary';
      case MovieDownloadStatusCode.Started:
        return 'fas fa-sync fa-spin';
    }
  }
}
