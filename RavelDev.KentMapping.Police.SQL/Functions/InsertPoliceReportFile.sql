-- FUNCTION: public.insertpolicereportfile()

-- DROP FUNCTION IF EXISTS public.insertpolicereportfile();

CREATE OR REPLACE FUNCTION public.insertpolicereportfile(
	IN filename text,
	OUT idToReturn integer)
    RETURNS integer
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE PARALLEL UNSAFE

AS $BODY$
BEGIN
	
	INSERT INTO public."PoliceCaseReportFile"("FileName") VALUES (fileName) RETURNING "PoliceCaseReportFileId" INTO idToReturn;


END
$BODY$;

ALTER FUNCTION public.insertpolicereportfile(
	IN filename text)
    OWNER TO kent_reader;
