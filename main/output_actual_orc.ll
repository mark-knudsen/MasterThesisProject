; ModuleID = 'repl_module'
source_filename = "repl_module"

%dataframe = type { ptr, ptr, ptr }
%array = type { i64, i64, ptr }

@x = external global ptr

define ptr @main_16() {
entry:
  %x_load = load ptr, ptr @x, align 8
  %rows_ptr_ptr = getelementptr inbounds nuw %dataframe, ptr %x_load, i32 0, i32 1
  %rows = load ptr, ptr %rows_ptr_ptr, align 8
  %data_ptr_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 2
  %data_ptr = load ptr, ptr %data_ptr_ptr, align 8
  %len_ptr = getelementptr inbounds nuw %array, ptr %rows, i32 0, i32 0
  %len = load i64, ptr %len_ptr, align 4
  %in_bounds = icmp ult i64 0, %len
  br i1 %in_bounds, label %idx_ok, label %idx_err

idx_ok:                                           ; preds = %entry
  %elem_ptr = getelementptr ptr, ptr %data_ptr, i64 0
  %record = load ptr, ptr %elem_ptr, align 8
  %runtime_obj = call ptr @malloc(i64 16)
  %runtime_cast = bitcast ptr %runtime_obj to ptr
  %tag_ptr = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 0
  store i16 6, ptr %tag_ptr, align 8
  %data_ptr1 = getelementptr inbounds nuw { i16, [6 x i8], ptr }, ptr %runtime_cast, i32 0, i32 2
  store ptr %record, ptr %data_ptr1, align 8
  ret ptr %runtime_obj

idx_err:                                          ; preds = %entry
  ret ptr null
}

declare i32 @printf(ptr, ...)

declare ptr @malloc(i64)
