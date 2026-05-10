using System;
using System.Collections.Generic;
using Newtonsoft.Json;

[Serializable]
public class MascotaRecord
{
    [JsonProperty("id")] public string id;
    [JsonProperty("nombre")] public string nombre;
    [JsonProperty("especie")] public string especie;
    [JsonProperty("raza")] public string raza;
    [JsonProperty("sexo")] public string sexo;
    [JsonProperty("foto_url")] public string foto_url;
    [JsonProperty("dueno_nombre")] public string dueno_nombre;
    [JsonProperty("telefono")] public string telefono;
    [JsonProperty("ciudad")] public string ciudad;
    [JsonProperty("email")] public string email;
    [JsonProperty("fecha_nacimiento")] public string fecha_nacimiento;
    [JsonProperty("vacunas")] public string vacunas;
    [JsonProperty("alimentacion")] public string alimentacion;
    [JsonProperty("otros")] public string otros;
}

[Serializable]
public class ApiResponse
{
    public bool ok;
    public int count;
    public List<MascotaRecord> data;
}