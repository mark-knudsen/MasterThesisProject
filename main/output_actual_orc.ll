; ModuleID = 'repl_module'
source_filename = "repl_module"

%RuntimeValue = type { i64, ptr }

@str = private unnamed_addr constant [49 x i8] c"CSV/Fire_Prediction_2023_Bolivia_encoded_10K.csv\00", align 1
@df = global ptr null

define ptr @main_0() {
entry:
  %df_raw_ptr = call ptr @ReadCsvInternal(ptr @str)
  %runtime_obj_raw = call ptr @malloc(i64 16)
  %runtime_obj_ptr = bitcast ptr %runtime_obj_raw to ptr
  %tag_ptr = getelementptr inbounds nuw %RuntimeValue, ptr %runtime_obj_ptr, i32 0, i32 0
  store i64 7, ptr %tag_ptr, align 4
  %data_ptr = getelementptr inbounds nuw %RuntimeValue, ptr %runtime_obj_ptr, i32 0, i32 1
  store ptr %df_raw_ptr, ptr %data_ptr, align 8
  store ptr %runtime_obj_ptr, ptr @df, align 8
  ret ptr %runtime_obj_ptr
}

declare i32 @printf(ptr, ...)

declare ptr @ReadCsvInternal(ptr)

declare ptr @malloc(i64)
