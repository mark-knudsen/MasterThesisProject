; ModuleID = 'repl_module'
source_filename = "repl_module"

%struct_name_age_hasJob = type { ptr, i64, i8 }

@r2 = external global ptr

define ptr @main_2() {
entry:
  %r2_load = load ptr, ptr @r2, align 8
  %ptr_name = getelementptr %struct_name_age_hasJob, ptr %r2_load, i32 0, i32 0
  %val_name = load ptr, ptr %ptr_name, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %tag_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 0
  store i64 4, ptr %tag_ptr, align 8
  %data_ptr = getelementptr inbounds nuw { i64, ptr }, ptr %runtime_obj, i32 0, i32 1
  store ptr %val_name, ptr %data_ptr, align 8
  ret ptr %runtime_obj
}

declare i32 @printf(ptr, ...)

declare noalias ptr @malloc(i64)
