namespace AppCitasPsicologia.Models.Paginacion
{
    public class PaginacionViewModel
    {
        public int Pagina { get; set; } = 1;
        public int recordPorPagina { get; set; } = 10;
        private readonly int cantidadMaximaRecordPorPagina = 50;
        public int RecordsPorPagina 
        {
            get 
            {
                return recordPorPagina;
            }
            set 
            {
                recordPorPagina = value > cantidadMaximaRecordPorPagina ? cantidadMaximaRecordPorPagina : value;
            }
        }
        public int RecordsASaltar => recordPorPagina * (Pagina - 1);
    }
}
