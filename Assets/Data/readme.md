# Attribution

Listed here are all data sources used in this project.


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
  12-01-2022, https://data.oecd.org/energy/electricity-generation.htm
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

**countries:** @mledoze/countries, "World countries in JSON, CSV, XML and YAML.", 12-01-2022, https://github.com/mledoze/countries

--- 

### Format
**[Folder]:** [Data Provider], "[Data/Dataset Name or ID]", [Date of download MM-DD-YYYY], [Data Source URL]

[^solid_biofuels]: Per [OECD](https://data.oecd.org/energy/renewable-energy.htm) definition,  
  "Biofuels are defined as fuels derived directly or indirectly from biomass (material obtained from living or recently living organisms). This includes wood, vegetal waste (including wood waste and crops used for energy production), ethanol, animal materials/wastes and sulphite lyes. Municipal waste comprises wastes produced by the residential, commercial and public service sectors that are collected by local authorities for disposal in a central location for the production of heat and/or power."
[^oecdusd]: OECD $USD financial measurements are made in US dollars and US dollars per capita (current [PPPs](https://en.wikipedia.org/wiki/Purchasing_power_parity)). For further information, refer to the OECD's [Economy Data Portal](https://data.oecd.org/economy.htm), or refer to the [OECD Economic Outlook](https://www.oecd-ilibrary.org/economics/oecd-economic-outlook_16097408) publication.
