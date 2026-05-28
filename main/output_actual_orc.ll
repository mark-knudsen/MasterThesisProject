; ModuleID = 'repl_module'
source_filename = "repl_module"

%struct_name_age = type { ptr, i64 }

@str = private unnamed_addr constant [6 x i8] c"Harry\00", align 1

define ptr @main_14() {
entry:
  %record_ptr = call ptr @malloc(i64 ptrtoint (ptr getelementptr (%struct_name_age, ptr null, i32 1) to i64))
  %field_0 = getelementptr %struct_name_age, ptr %record_ptr, i32 0, i32 0
  store ptr @str, ptr %field_0, align 8
  %field_1 = getelementptr %struct_name_age, ptr %record_ptr, i32 0, i32 1
  store i64 900, ptr %field_1, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 6, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %record_ptr, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
