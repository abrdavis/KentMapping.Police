-- FUNCTION: public.insertpoliceincidentfile()

-- DROP FUNCTION IF EXISTS public.insertpoliceincidentfile();

CREATE OR REPLACE FUNCTION public.insertpoliceincidentfile(
	IN filename text,
	OUT idToReturn integer)
    RETURNS integer
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE PARALLEL UNSAFE

AS $BODY$
BEGIN
	
	INSERT INTO public."PoliceIncidentFile"("FileName") VALUES (fileName) RETURNING "PoliceIncidentFileId" INTO idToReturn;


END
$BODY$;

ALTER FUNCTION public.insertpoliceincidentfile(
	IN filename text)
    OWNER TO kent_reader;
