# Attribution

Listed here are all data sources used in this project.[^citations]  

The data itself is located in the Resources folder, for edit-time loading in the Unity engine.  

It is separated into City and Country subdirectories.


## Databases

Databases from which multiple datasets were retrieved


### OECD
**oecd/**  
The Organisation for Economic Co-operation and Development (OECD)  
https://data.oecd.org/  

#### Energy
**oecd/energy/**  
Topics related to the energy sector (generation, consumption, import, export, etc)  
https://data.oecd.org/energy.htm  

- **crudeoil_prod:** Crude oil production per country, measured in thousands [TOE](https://en.wikipedia.org/wiki/Tonne_of_oil_equivalent)  
  12-01-2022, https://data.oecd.org/energy/crude-oil-production.htm
- **elecgen_total:** Energy generation (total) per country, measured in [GWh](https://en.wikipedia.org/wiki/Kilowatt-hour#Watt-hour_multiples)  
  12-01-2022, https://data.oecd.org/energy/electricity-generation.htm
- **elecgen_nuc_gwh:** Energy generation (nuclear) per country, measured in [GWh](https://en.wikipedia.org/wiki/Kilowatt-hour#Watt-hour_multiples)  
  12-01-2022, https://data.oecd.org/energy/electricity-generation.htm
- **elecgen_nuc_per:** Energy generation (nuclear) per country, measured as a percentage of total energy generation  
  12-06-2022, https://data.oecd.org/energy/electricity-generation.htm
- **nuclearplants_country:** Number of nuclear power generation units in operation as of 1 January 2019 per country
  12-01-2022, https://data.oecd.org/energy/nuclear-power-plants.htm
- **renewable_per:** Renewable energy per country, measured as a percent of its contribution to total primary energy supply
  12-01-2022, https://data.oecd.org/energy/renewable-energy.htm
- **renewable_excl_solid_biofuel_per:** Renewable energy (excluding solid biofuels[^solid_biofuels]) per country, measured as a percent of its contribution to total primary energy supply
  12-01-2022, https://data.oecd.org/energy/renewable-energy.htm
- **renewable_ttoe:** Renewable energy production per country, measured in thousands [TOE](https://en.wikipedia.org/wiki/Tonne_of_oil_equivalent)  
  12-01-2022, https://data.oecd.org/energy/renewable-energy.htm
  
#### Economic
**oecd/economic/**  
Topics related to the economic sector (GDP, income/expense, investment, debt, productivity, etc)  
https://data.oecd.org/economy.htm

- **gdp_usdpcap:** Gross Domestic Product per country, measured in $USD[^oecdusd] per capita  
  12-01-2022, https://data.oecd.org/gdp/gross-domestic-product-gdp.htm
- **gdp_usdmil:** Gross Domestic Product per country, measured in millions of $USD[^oecdusd]  
  12-01-2022, https://data.oecd.org/gdp/gross-domestic-product-gdp.htm
- **net_lendborrow_corp:** Net lending/borrowing (corporate sector) per country, measured as a percent of GDP  
  12-01-2022, https://data.oecd.org/natincome/net-lending-borrowing-by-sector.htm
- **net_lendborrow_govt:** Net lending/borrowing (government sector) per country, measured as a percent of GDP  
  12-01-2022, https://data.oecd.org/natincome/net-lending-borrowing-by-sector.htm
- **net_lendborrow_house:** Net lending/borrowing (household sector) per country, measured as a percent of GDP  
  12-01-2022, https://data.oecd.org/natincome/net-lending-borrowing-by-sector.htm
- **net_lendborrow_total:** Net lending/borrowing (total) per country, measured as a percent of GDP  
  12-01-2022, https://data.oecd.org/natincome/net-lending-borrowing-by-sector.htm


## Individual Datasets

Single datasets, or databases from which only one set of data was retrieved

- **cities1000:** GeoNames, "All Cities with a population > 1000", 11-30-2022, https://download.geonames.org/export/dump/  
- **worldairports:** OurAirports, "airports.csv", 11-30-2022, https://ourairports.com/data/  
- **laureates:** Nobel Prize, "Nobel Prize - Laureates", 12-01-2022, https://www.nobelprize.org/about/developer-zone-2/  


## Other Data

- **countries:** @mledoze/countries, "World countries in JSON, CSV, XML and YAML.", 12-01-2022, https://github.com/mledoze/countries  
- **continents:** datahub.io, "List of continent codes", 12-06-2022, https://datahub.io/core/continent-codes  

## Unsorted Data
Data that has been collected, but has yet to be formatted into the sorted lists above. 

- dec1 oecd/economy https://data.oecd.org/energy.htm 
  - fincorp_debt_equity_ratio https://data.oecd.org/corporate/financial-corporations-debt-to-equity-ratio.htm
  - fdi_stocks_in/out_pergdp/usdmil-country-latest https://data.oecd.org/fdi/fdi-stocks.htm
  - fdi_in/out_usdpcap/usdmil-country-latest https://data.oecd.org/fdi/fdi-flows.htm
  - fdi_restrictiveness-country-latest https://data.oecd.org/fdi/fdi-restrictiveness.htm
- dec1 oecd/agricultural https://data.oecd.org/agriculture.htm dec1 
  - crop_production-country-total https://data.oecd.org/agroutput/crop-production.htm
  - meat_consumption-country-total https://data.oecd.org/agroutput/meat-consumption.htm
  - agripolicy_support_total_pergdp-country-latest https://data.oecd.org/agrpolicy/agricultural-support.htm (TSE, %)
  - agripolicy_prodprot-country-latest https://data.oecd.org/agrpolicy/producer-protection.htm
  - fish_landings_total_usd/tonnes-country https://data.oecd.org/fish/fish-landings.htm
  - aqua_prod_usd/tonnes-country-latest https://data.oecd.org/fish/aquaculture-production.htm
  - nutrientbalance_n/p_kgh-country-latest https://data.oecd.org/agrland/nutrient-balance.htm
  - agriland_total-country-latest https://data.oecd.org/agrland/agricultural-land.htm
- dec1 oecd/education https://data.oecd.org/education.htm dec1 
  - eduleveladult_e1/e2/e3_per25to64-country-latest https://data.oecd.org/eduatt/adult-education-level.htm
  - eduspend_pub_e1e2e3_pergdp-country-latest https://data.oecd.org/eduresource/private-spending-on-education.htm
  - eduspend_pvt_e1e2e3_pergdp-country-latest https://data.oecd.org/eduresource/private-spending-on-education.htm
  - readingperf_mean-country-latest https://data.oecd.org/pisa/reading-performance-pisa.htm
  - mathperf_mean-country-latest https://data.oecd.org/pisa/mathematics-performance-pisa.htm
  - scienceperf_mean-country-latest https://data.oecd.org/pisa/science-performance-pisa.htm
  - gradrate_e2_mwt-country-latest https://data.oecd.org/students/secondary-graduation-rate.htm
  - gradrate_e3_doc/mas/bac/total_mw/mwt-country-latest https://data.oecd.org/students/tertiary-graduation-rate.htm
  - enrolment_e0_3y4y5y-country-total https://data.oecd.org/students/enrolment-rate-in-early-childhood-education.htm
  - enrolment_e2e3_17y18y19y-country-total https://data.oecd.org/students/enrolment-rate-in-secondary-and-tertiary-education.htm
  - enrolment_e3_per_intl-country-latest https://data.oecd.org/students/international-student-mobility.htm
  - teacher_salary_e0/e1/e2/e3-country-latest https://data.oecd.org/teachers/teachers-salaries.htm
  - teacher_studentsperstaff https://data.oecd.org/teachers/students-per-teaching-staff.htm
  - teacher_perwomen https://data.oecd.org/teachers/women-teachers.htm
  - teacher_perage_e1/e2/e3 https://data.oecd.org/teachers/teachers-by-age.htm
  - teacher_hrsperyr https://data.oecd.org/teachers/teaching-hours.htm
  - teacher_staff https://data.oecd.org/teachers/teaching-staff.htm
  - teacher_principals_avgage/perwomen/yrsexp https://data.oecd.org/teachers/school-principals.htm
  - youth_neet_total/m/w-country-latest https://data.oecd.org/youthinac/youth-not-in-employment-education-or-training-neet.htm
- dec1 environment https://data.oecd.org/environment.htm  
  - airpoll_exposure-country-latest https://data.oecd.org/air/air-pollution-exposure.htm
  - airpoll_mortper1m-country-latest https://data.oecd.org/air/air-pollution-effects.htm
  - emm_co2/conoxsoxvoc/ghg_milton/toncap/kgpcap/thouton/thoutonco2eq https://data.oecd.org/air/air-and-ghg-emissions.htm
  - biodiv_landcoverchange_s2004/s1992 https://data.oecd.org/biodiver/land-cover-change.htm
  - biodiv_protectedarea_land/water https://data.oecd.org/biodiver/protected-areas.htm
  - biodiv_threatenedspecies https://data.oecd.org/biodiver/threatened-species.htm
  - forest_treefellings https://data.oecd.org/forest/forest-resources.htm
  - mats_cons https://data.oecd.org/materials/material-consumption.htm
  - mats_prod https://data.oecd.org/materials/material-productivity.htm
  - muniwaste_kgpcap/thouton https://data.oecd.org/waste/municipal-waste.htm
  - water_withdrawal_m3mil/m3pcap https://data.oecd.org/water/water-withdrawals.htm
  - water_wastewatertreatment https://data.oecd.org/water/wastewater-treatment.htm
- dev6 environment https://data.oecd.org/environment.htm  
  - biodiv_builtuparea https://data.oecd.org/biodiver/built-up-area.htm
- govt https://data.oecd.org/government.htm dec1 
  - govt_trust https://data.oecd.org/gga/trust-in-government.htm
  - govt_deficit https://data.oecd.org/gga/general-government-deficit.htm
  - govt_revenue_pergdp/thouusdpcap https://data.oecd.org/gga/general-government-revenue.htm
  - govt_spend_bydestination https://data.oecd.org/gga/general-government-spending-by-destination.htm
  - govt_spend_general_pergdp/thouusdpcap_pg1/pg2/total https://data.oecd.org/gga/general-government-spending.htm
  - govt_spend_central_pg1/pg2 https://data.oecd.org/gga/central-government-spending.htm
  - govt_debt https://data.oecd.org/gga/general-government-debt.htm
  - govt_wealth https://data.oecd.org/gga/general-government-financial-wealth.htm
  - govt_productioncosts https://data.oecd.org/gga/government-production-costs.htm
  - tax_revenue_pergdp/milusd https://data.oecd.org/tax/tax-revenue.htm
  - tax_persincome_pergdp/pertax https://data.oecd.org/tax/tax-on-personal-income.htm
  - tax_corp_pergdp/pertax https://data.oecd.org/tax/tax-on-corporate-profits.htm
  - tax_socsec_pergdp/pertax https://data.oecd.org/tax/social-security-contributions.htm
  - tax_payroll_pergdp/pertax https://data.oecd.org/tax/tax-on-payroll.htm
  - tax_prop_pergdp/pertax https://data.oecd.org/tax/tax-on-property.htm
  - tax_goods_pergdp/pertax https://data.oecd.org/tax/tax-on-goods-and-services.htm
- health dec1 https://data.oecd.org/health.htm
  - health_use_docvisitspcap https://data.oecd.org/healthcare/doctors-consultations.htm
  - health_use_childvaccrates https://data.oecd.org/healthcare/child-vaccination-rates.htm
  - health_use_fluvacc65plus https://data.oecd.org/healthcare/influenza-vaccination-rates.htm
  - health_use_hospitalstaylength https://data.oecd.org/healthcare/length-of-hospital-stay.htm
  - health_use_hospitaldischarge https://data.oecd.org/healthcare/hospital-discharge-rates.htm
  - health_csecsper1k https://data.oecd.org/healthcare/caesarean-sections.htm
  - health_equip_hospitalbedsper1k https://data.oecd.org/healtheqt/hospital-beds.htm
  - health_res_spending_pergdp/usdpcap/pertotal https://data.oecd.org/healthres/health-spending.htm
  - health_res_doctorsper1k https://data.oecd.org/healthres/doctors.htm
  - health_res_nursesper1k https://data.oecd.org/healthres/nurses.htm
  - health_res_medgradsper100k https://data.oecd.org/healthres/medical-graduates.htm
  - health_res_nursinggradsper100k https://data.oecd.org/healthres/nursing-graduates.htm
  - health_risk_dailysmokers https://data.oecd.org/healthrisk/daily-smokers.htm
  - health_risk_alcohol https://data.oecd.org/healthrisk/alcohol-consumption.htm
  - health_risk_obesity https://data.oecd.org/healthrisk/overweight-or-obese-population.htm
  - health_risk_socsupp_total/gender/edu/age https://data.oecd.org/healthrisk/social-support.htm
  - health_stat_lifeexpect https://data.oecd.org/healthstat/life-expectancy-at-birth.htm
  - health_stat_lifeexpect65 https://data.oecd.org/healthstat/life-expectancy-at-65.htm
  - health_stat_infantmort https://data.oecd.org/healthstat/infant-mortality-rates.htm
  - health_stat_pyll https://data.oecd.org/healthstat/potential-years-of-life-lost.htm
  - health_stat_cancerdeaths https://data.oecd.org/healthstat/deaths-from-cancer.htm
  - health_stat_suicidesper100k https://data.oecd.org/healthstat/suicide-rates.htm
- infotech dec2 https://data.oecd.org/innovation-and-technology.htm
  - broadband_fixedsubs_per100 https://data.oecd.org/broadband/fixed-broadband-subscriptions.htm
  - broadband_mobilesubs_per100 ttps://data.oecd.org/broadband/mobile-broadband-subscriptions.htm
  - employ_empbybusinesssize_total/bynumofemp https://data.oecd.org/entrepreneur/employees-by-business-size.htm
  - employ_selfemployed_20to29mw https://data.oecd.org/entrepreneur/young-self-employed.htm
  - employ_selfemp_withemp https://data.oecd.org/entrepreneur/self-employed-with-employees.htm
  - employ_selfemp_withoutemp https://data.oecd.org/entrepreneur/self-employed-without-employees.htm
  - employ_runningabusiness https://data.oecd.org/entrepreneur/running-a-business.htm
  - tourism_pergdp/pergva https://data.oecd.org/industry/tourism-gdp.htm
  - ict_homeinternetaccess https://data.oecd.org/ict/internet-access.htm
  - itc_homecomputeraccess https://data.oecd.org/ict/access-to-computers-from-home.htm
  - ict_valueadd https://data.oecd.org/ict/ict-value-added.htm
  - ict_employment https://data.oecd.org/ict/ict-employment.htm
  - research_rdspend_pergdp/usdmil https://data.oecd.org/rd/gross-domestic-spending-on-r-d.htm
  - research_researchers_total/women_per1k/count/percent https://data.oecd.org/rd/researchers.htm
  - research_govt_count/percent https://data.oecd.org/rd/government-researchers.htm
- infotech dec6 https://data.oecd.org/innovation-and-technology.htm
  - broadband_houseaccess https://data.oecd.org/broadband/households-with-broadband-access.htm
  - ict_goodsexport https://data.oecd.org/ict/ict-goods-exports.htm
  - ict_investment https://data.oecd.org/ict/ict-investment.htm
- jobs dec2 https://data.oecd.org/jobs.htm
  - ben_adequacyofminben https://data.oecd.org/benwage/adequacy-of-minimum-income-benefits.htm
  - ben_workhrspoverty_min/67avg/avg https://data.oecd.org/benwage/working-hours-needed-to-exit-poverty.htm
  - ben_disincentiveworkhours https://data.oecd.org/benwage/financial-disincentive-to-increase-working-hours.htm
  - ben_childcarecost_peravg/perhouse https://data.oecd.org/benwage/net-childcare-costs.htm
  - ben_disincentive_jobwithchildcare https://data.oecd.org/benwage/financial-disincentive-to-enter-employment-with-childcare-costs.htm
  - earn_avgwages https://data.oecd.org/earnwage/average-wages.htm
  - earn_compenactivity https://data.oecd.org/earnwage/employee-compensation-by-activity.htm
  - earn_wagegap https://data.oecd.org/earnwage/gender-wage-gap.htm
  - earn_levels https://data.oecd.org/earnwage/wage-levels.htm
  - employ_rate_percent/thou https://data.oecd.org/emp/employment-rate.htm
  - employ_byage https://data.oecd.org/emp/employment-rate-by-age-group.htm
  - employ_byedu https://data.oecd.org/emp/employment-by-education-level.htm
  - employ_parttime https://data.oecd.org/emp/part-time-employment-rate.htm
  - employ_self https://data.oecd.org/emp/self-employment-rate.htm
  - employ_labourforce https://data.oecd.org/emp/labour-force.htm
  - ump_rate https://data.oecd.org/unemp/unemployment-rate.htm
  - ump_rateage https://data.oecd.org/unemp/unemployment-rate-by-age-group.htm
  - ump_rateedu https://data.oecd.org/unemp/unemployment-rates-by-education-level.htm
  - ump_longterm https://data.oecd.org/unemp/long-term-unemployment-rate.htm
  - ump_youth https://data.oecd.org/unemp/youth-unemployment-rate.htm
- society dec2 https://data.oecd.org/society.htm

--- 

### Format
**[Folder]:** [Data Provider], "[Data/Dataset Name or ID]", [Date of download MM-DD-YYYY], [Data Source URL]

[^citations]: Attribution/citation. While data sources are attributed and linked, formal citations as required by the datasets have not yet been integrated. These will be added as soon as possible, but currently you can view the linked source website for citation information.
[^solid_biofuels]: Per [OECD](https://data.oecd.org/energy/renewable-energy.htm) definition,  
  "Biofuels are defined as fuels derived directly or indirectly from biomass (material obtained from living or recently living organisms). This includes wood, vegetal waste (including wood waste and crops used for energy production), ethanol, animal materials/wastes and sulphite lyes. Municipal waste comprises wastes produced by the residential, commercial and public service sectors that are collected by local authorities for disposal in a central location for the production of heat and/or power."
[^oecdusd]: OECD $USD financial measurements are made in US dollars and US dollars per capita (current [PPPs](https://en.wikipedia.org/wiki/Purchasing_power_parity)). For further information, refer to the OECD's [Economy Data Portal](https://data.oecd.org/economy.htm), or refer to the [OECD Economic Outlook](https://www.oecd-ilibrary.org/economics/oecd-economic-outlook_16097408) publication.
