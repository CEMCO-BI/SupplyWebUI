import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule, ReactiveFormsModule  } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { ToastrModule } from 'ngx-toastr';
import { UploadService } from './service/upload.service';


import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { HomeComponent } from './home/home.component';
import { UploadFileComponent } from './upload-file/upload-file.component';
import { GlobalConstants } from './common/global-constant'
import { AgGridModule } from 'ag-grid-angular'



@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    UploadFileComponent
    
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    AgGridModule.withComponents([]),
    RouterModule.forRoot([
      { path: '', component: UploadFileComponent, pathMatch: 'full' },
      { path: 'SalesForecast/:typo', component: UploadFileComponent, pathMatch: 'full' },
      { path: 'CRUPricing/:typo', component: UploadFileComponent, pathMatch: 'full' },
      { path: 'PlannedBuy/:typo', component: UploadFileComponent, pathMatch: 'full' },
      { path: 'MarginTables/:typo', component: UploadFileComponent, pathMatch: 'full' }

 
      //{ path: 'crupricing', component: CRUPricingComponent }
     // { path: 'upload-file', component: UploadFileComponent }
    ]),
    BrowserAnimationsModule,
    ToastrModule.forRoot({
      timeOut: 2000,
      progressBar: true,
      preventDuplicates: true,
      positionClass: 'toast-top-center',
      maxOpened: 1
    })
  ],
  providers: [UploadService],
  bootstrap: [AppComponent]
})
export class AppModule { }
