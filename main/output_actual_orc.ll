; ModuleID = 'repl_module'
source_filename = "repl_module"

@str = private unnamed_addr constant [50 x i8] c"CSV/Fire_Prediction_2023_Bolivia_encoded_FULL.csv\00", align 1
@csv_schema = private unnamed_addr constant [38 x i8] c"SFFFFFFFFFFFFFFFFFFFIBBBBBBBBBBBBBBBB\00", align 1
@df = global ptr null

define ptr @main_0() {
entry:
  %csv_boxed = call ptr @ReadCsvInternal(ptr @str, ptr @csv_schema)
  %0 = getelementptr inbounds nuw { i64, ptr }, ptr %csv_boxed, i32 0, i32 1
  %df_ptr = load ptr, ptr %0, align 8
  store ptr %df_ptr, ptr @df, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 0
  store i64 0, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_cast, i32 0, i32 1
  store ptr null, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare ptr @ReadCsvInternal(ptr, ptr)

declare ptr @malloc(i64)
