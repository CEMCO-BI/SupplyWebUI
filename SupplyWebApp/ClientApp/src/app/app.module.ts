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
import { GlobalConstants } from './common/global-constant';
import { PlannedBuyComponent } from './planned-buy/planned-buy.component';
import { SalesForecastComponent } from './sales-forecast/sales-forecast.component';
import { CRUPricingComponent } from './crupricing/crupricing.component'


@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    HomeComponent,
    UploadFileComponent,
    PlannedBuyComponent,
    SalesForecastComponent,
    CRUPricingComponent
    
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule.forRoot([
      { path: '', component: SalesForecastComponent, pathMatch: 'full' },
      { path: 'crupricing', component: CRUPricingComponent },
      { path: 'salesforecast', component: SalesForecastComponent },
      { path: 'plannedbuy', component: PlannedBuyComponent },
      { path: 'upload-file', component: UploadFileComponent }
    ]),
    BrowserAnimationsModule,
    ToastrModule.forRoot({
      timeOut: 2000,
      progressBar: true,
      preventDuplicates: true,
      positionClass: 'toast-top-center'
    })
  ],
  providers: [UploadService],
  bootstrap: [AppComponent]
})
export class AppModule { }
