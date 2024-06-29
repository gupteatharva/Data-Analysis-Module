import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { AppRoutingModule } from './app.routing';
import { ComponentsModule } from './components/components.module';
import { AppComponent } from './app.component';
import { AdminLayoutComponent } from './layouts/admin-layout/admin-layout.component';
import { MyMaterialModule } from  './material.module';
import { CsvListComponent } from './csv-list/csv-list.component';
import { CsvsListComponent } from './csvs-list/csvs-list.component';
import { DataSourcesComponent } from './data-sources/data-sources.component';
import { DataAnalysisComponent } from './data-analysis/data-analysis.component';
import { TableListComponent } from './table-list/table-list.component';
import { TableComponent } from './table/table.component';
import { ActionsComponent } from './actions/actions.component';
import { ViewTableComponent } from './view-table/view-table.component';
import { TypographyComponent } from './typography/typography.component';
import { GraphVisComponent } from './graph-vis/graph-vis.component';
import { GraphsComponent } from './graphs/graphs.component';
import { DashboardComponent } from './dashboard/dashboard.component';

@NgModule({
  imports: [
    BrowserAnimationsModule,
    FormsModule,
    ReactiveFormsModule,
    HttpClientModule,
    ComponentsModule,
    RouterModule,
    AppRoutingModule,
    MyMaterialModule,
  ],
  declarations: [
    AppComponent,
    AdminLayoutComponent,
    CsvListComponent,
    CsvsListComponent,
    DataSourcesComponent,
    DataAnalysisComponent,
    TableListComponent,
    TableComponent,
    ActionsComponent,
    ViewTableComponent,
    TypographyComponent,
    GraphVisComponent,
    GraphsComponent,
    DashboardComponent
    

  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
